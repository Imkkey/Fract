using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Player Combat" )]
[Category( "Battle Royale" )]
public sealed class PlayerCombat : Component
{
	const string DeadRagdollTag = "deadplayer";
	const string PlayerTag = "player";

	[Property] public float BaseMaxHealth { get; set; } = 100f;
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public float DefaultDeathImpulse { get; set; } = 280f;
	[Property] public float UpwardDeathImpulse { get; set; } = 80f;
	[Property, Group( "Debug" )] public float DebugHealthDelta { get; set; } = 10f;

	[Sync( Flags = SyncFlags.FromHost )] public float MaxHealth { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public float Health { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public bool IsDead { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public float PhysicalDamageBonusPercent { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public float MagicDamageBonusPercent { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public float CooldownReductionPercent { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public int BonusAmmoCapacity { get; private set; }

	Vector3 PendingRagdollHitPosition { get; set; }
	Vector3 PendingRagdollImpulse { get; set; }
	int PendingRagdollImpulseFrames { get; set; }
	bool DeathRagdollStarted { get; set; }
	GameObject DeathRagdollObject { get; set; }
	ModelPhysics DeathRagdollPhysics { get; set; }
	List<ModelRenderer> HiddenLiveRenderers { get; } = new();

	protected override void OnStart()
	{
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );
		TagGameplayColliders();

		if ( Networking.IsHost )
		{
			ResetForMatch();
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( IsDead )
		{
			SetGameplayCollidersEnabled( false );
		}

		if ( PendingRagdollImpulseFrames <= 0 )
			return;

		var ragdoll = DeathRagdollPhysics;
		ConfigureDeadRagdollCollision( ragdoll );
		if ( !ragdoll.IsValid() || ApplyRagdollImpulse( ragdoll, PendingRagdollHitPosition, PendingRagdollImpulse ) )
		{
			ClearPendingRagdollImpulse();
			return;
		}

		PendingRagdollImpulseFrames--;
	}

	protected override void OnDestroy()
	{
		DestroyLocalRagdoll();
	}

	public void ResetForMatch()
	{
		MaxHealth = BaseMaxHealth;
		Health = MaxHealth;
		IsDead = false;
		PhysicalDamageBonusPercent = 0f;
		MagicDamageBonusPercent = 0f;
		CooldownReductionPercent = 0f;
		BonusAmmoCapacity = 0;
		GameObject.Enabled = true;
		ResetDeathState();
	}

	public float ScaleDamage( float baseDamage, DamageType damageType )
	{
		var bonus = damageType == DamageType.Magic ? MagicDamageBonusPercent : PhysicalDamageBonusPercent;
		return baseDamage * (1f + MathF.Max( 0f, bonus ) / 100f);
	}

	public float ScaleCooldown( float baseCooldown )
	{
		var reduction = CooldownReductionPercent.Clamp( 0f, 80f ) / 100f;
		return baseCooldown * (1f - reduction);
	}

	public void ApplyBuff( BuffType type, float value )
	{
		if ( !Networking.IsHost || IsDead )
			return;

		switch ( type )
		{
			case BuffType.PhysicalDamagePercent:
				PhysicalDamageBonusPercent += value;
				break;
			case BuffType.MagicDamagePercent:
				MagicDamageBonusPercent += value;
				break;
			case BuffType.MaxHealth:
				MaxHealth += value;
				Health = MathF.Min( MaxHealth, Health + value );
				break;
			case BuffType.CooldownReductionPercent:
				CooldownReductionPercent += value;
				break;
			case BuffType.AmmoCapacity:
				BonusAmmoCapacity += (int)value;
				break;
		}
	}

	public void ApplyDamage( DamageEvent damageEvent )
	{
		if ( !Networking.IsHost || IsDead )
			return;

		Health = MathF.Max( 0f, Health - damageEvent.Amount );

		if ( Health <= 0f )
		{
			Kill( damageEvent );
		}
	}

	public void RequestDebugHealthDelta( float delta )
	{
		ApplyDebugHealthDelta( delta );
	}

	void ApplyDebugHealthDelta( float delta )
	{
		if ( !Networking.IsHost || IsDead )
			return;

		Health = (Health + delta).Clamp( 0f, MaxHealth );
	}

	[Button( "Debug Add HP" ), Group( "Debug" )]
	public void DebugAddHealth()
	{
		RequestDebugHealthDelta( MathF.Abs( DebugHealthDelta ) );
	}

	[Button( "Debug Remove HP" ), Group( "Debug" )]
	public void DebugRemoveHealth()
	{
		RequestDebugHealthDelta( -MathF.Abs( DebugHealthDelta ) );
	}

	void Kill( DamageEvent damageEvent )
	{
		IsDead = true;

		var hitPosition = damageEvent.HitPosition;
		var impulse = GetDeathImpulse( damageEvent );

		PlayDeathRagdollLocal( hitPosition, impulse );
		BroadcastDeathRagdoll( hitPosition, impulse );
		BattleRoyaleMatch.Current?.NotifyPlayerDied( this, damageEvent.Source );
	}

	[Rpc.Broadcast]
	void BroadcastDeathRagdoll( Vector3 hitPosition, Vector3 impulse )
	{
		PlayDeathRagdollLocal( hitPosition, impulse );
	}

	void PlayDeathRagdollLocal( Vector3 hitPosition, Vector3 impulse )
	{
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndDescendants );
		SetGameplayEnabled( false );
		SpawnLocalDeathRagdoll( hitPosition, impulse );
		SetLiveRenderersVisible( false );
	}

	void SetGameplayEnabled( bool enabled )
	{
		var controller = GetComponent<PlayerController>();
		if ( controller.IsValid() )
		{
			controller.Enabled = enabled;
		}

		var attackController = GetComponent<PlayerAttackController>();
		if ( attackController.IsValid() )
		{
			attackController.Enabled = enabled;
		}

		var clubWeapon = GetComponent<ClubWeaponIk>();
		if ( clubWeapon.IsValid() )
		{
			clubWeapon.Enabled = enabled;
		}

		var body = GetComponent<Rigidbody>();
		if ( body.IsValid() )
		{
			body.MotionEnabled = enabled;
		}

		SetGameplayCollidersEnabled( enabled );
	}

	void SetGameplayCollidersEnabled( bool enabled )
	{
		foreach ( var collider in GameObject.Components.GetAll<Collider>( FindMode.EverythingInSelfAndDescendants ) )
		{
			if ( !collider.IsValid() )
				continue;

			collider.GameObject.Tags.Add( PlayerTag );
			collider.Enabled = enabled;
		}
	}

	void TagGameplayColliders()
	{
		GameObject.Tags.Add( PlayerTag );

		foreach ( var collider in GameObject.Components.GetAll<Collider>( FindMode.EverythingInSelfAndDescendants ) )
		{
			if ( collider.IsValid() )
				collider.GameObject.Tags.Add( PlayerTag );
		}
	}

	void SpawnLocalDeathRagdoll( Vector3 hitPosition, Vector3 impulse )
	{
		if ( DeathRagdollStarted )
			return;

		if ( !BodyRenderer.IsValid() || !BodyRenderer.Model.IsValid() )
			return;

		DeathRagdollStarted = true;

		DestroyLocalRagdoll();

		var ragdollObject = new GameObject( false, $"{GameObject.Name} Death Ragdoll" );
		ragdollObject.NetworkMode = NetworkMode.Never;
		ragdollObject.Tags.Add( DeadRagdollTag );
		ragdollObject.WorldTransform = BodyRenderer.WorldTransform;

		var ragdollRenderer = ragdollObject.Components.Create<SkinnedModelRenderer>( false );
		ragdollRenderer.CopyFrom( BodyRenderer );
		ragdollRenderer.Model = BodyRenderer.Model;
		ragdollRenderer.UseAnimGraph = false;
		ragdollRenderer.WorldTransform = BodyRenderer.WorldTransform;
		ragdollRenderer.Enabled = true;
		CloneBoneMergedRenderers( ragdollObject, ragdollRenderer );

		var ragdoll = ragdollObject.Components.Create<ModelPhysics>( false );
		ragdoll.Enabled = false;
		ragdoll.Renderer = ragdollRenderer;
		ragdoll.Model = BodyRenderer.Model;
		ragdoll.StartAsleep = false;
		ragdoll.IgnoreRoot = false;
		ragdoll.MotionEnabled = false;

		ragdollObject.Enabled = true;
		ragdoll.Enabled = true;
		ragdoll.CopyBonesFrom( BodyRenderer, true );
		ragdoll.MotionEnabled = true;

		DeathRagdollObject = ragdollObject;
		DeathRagdollPhysics = ragdoll;
		ConfigureDeadRagdollCollision( ragdoll );

		if ( !ApplyRagdollImpulse( ragdoll, hitPosition, impulse ) )
		{
			QueueRagdollImpulse( hitPosition, impulse );
		}
	}

	void CloneBoneMergedRenderers( GameObject ragdollObject, SkinnedModelRenderer ragdollRenderer )
	{
		foreach ( var sourceRenderer in GameObject.Components.GetAll<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InSelf | FindMode.InDescendants ) )
		{
			if ( !sourceRenderer.IsValid() || sourceRenderer == BodyRenderer )
				continue;

			if ( sourceRenderer.BoneMergeTarget != BodyRenderer || !sourceRenderer.Model.IsValid() )
				continue;

			var rendererObject = new GameObject( ragdollObject, false, sourceRenderer.GameObject.Name );
			rendererObject.NetworkMode = NetworkMode.Never;
			rendererObject.Tags.Add( DeadRagdollTag );
			rendererObject.WorldTransform = sourceRenderer.WorldTransform;

			var clonedRenderer = rendererObject.Components.Create<SkinnedModelRenderer>( false );
			clonedRenderer.CopyFrom( sourceRenderer );
			clonedRenderer.Model = sourceRenderer.Model;
			clonedRenderer.BoneMergeTarget = ragdollRenderer;
			clonedRenderer.WorldTransform = sourceRenderer.WorldTransform;
			clonedRenderer.Enabled = true;

			rendererObject.Enabled = true;
		}
	}

	void ConfigureDeadRagdollCollision( ModelPhysics ragdoll )
	{
		if ( !ragdoll.IsValid() || ragdoll.Bodies is null )
			return;

		foreach ( var ragdollBody in ragdoll.Bodies )
		{
			var body = ragdollBody.Component;
			if ( !body.IsValid() )
				continue;

			body.GameObject.Tags.Add( DeadRagdollTag );
			body.CollisionEventsEnabled = false;
			body.CollisionUpdateEventsEnabled = false;
			body.EnableImpactDamage = false;

			foreach ( var collider in body.GameObject.Components.GetAll<Collider>( FindMode.EverythingInSelfAndDescendants ) )
			{
				if ( !collider.IsValid() )
					continue;

				collider.GameObject.Tags.Add( DeadRagdollTag );
				collider.ColliderFlags |= ColliderFlags.IgnoreTraces;
			}
		}
	}

	void SetLiveRenderersVisible( bool visible )
	{
		if ( visible )
		{
			foreach ( var renderer in HiddenLiveRenderers )
			{
				if ( renderer.IsValid() )
					renderer.Enabled = true;
			}

			HiddenLiveRenderers.Clear();
			return;
		}

		if ( HiddenLiveRenderers.Count > 0 )
			return;

		foreach ( var renderer in GameObject.Components.GetAll<ModelRenderer>( FindMode.Enabled | FindMode.InSelf | FindMode.InDescendants ) )
		{
			if ( !renderer.IsValid() )
				continue;

			HiddenLiveRenderers.Add( renderer );
			renderer.Enabled = false;
		}
	}

	void DestroyLocalRagdoll()
	{
		if ( DeathRagdollObject.IsValid() )
		{
			DeathRagdollObject.Destroy();
		}

		DeathRagdollObject = null;
		DeathRagdollPhysics = null;
	}

	bool ApplyRagdollImpulse( ModelPhysics ragdoll, Vector3 hitPosition, Vector3 impulse )
	{
		if ( impulse.Length <= 0.01f )
			return true;

		if ( ragdoll.Bodies is null || ragdoll.Bodies.Count == 0 )
			return false;

		var bodyCount = MathF.Max( 1f, ragdoll.Bodies.Count );
		var applied = false;
		foreach ( var ragdollBody in ragdoll.Bodies )
		{
			var body = ragdollBody.Component;
			if ( !body.IsValid() )
				continue;

			body.MotionEnabled = true;
			body.Sleeping = false;
			body.ApplyImpulseAt( hitPosition, impulse / bodyCount );
			applied = true;
		}

		return applied;
	}

	Vector3 GetDeathImpulse( DamageEvent damageEvent )
	{
		if ( damageEvent.Impulse.Length > 0.01f )
			return damageEvent.Impulse;

		var direction = Vector3.Zero;
		if ( damageEvent.Source.IsValid() )
		{
			direction = (WorldPosition - damageEvent.Source.WorldPosition).WithZ( 0f );
		}

		if ( direction.Length <= 0.01f )
		{
			direction = WorldRotation.Forward.WithZ( 0f );
		}

		return direction.Normal * DefaultDeathImpulse + Vector3.Up * UpwardDeathImpulse;
	}

	void QueueRagdollImpulse( Vector3 hitPosition, Vector3 impulse )
	{
		PendingRagdollHitPosition = hitPosition;
		PendingRagdollImpulse = impulse;
		PendingRagdollImpulseFrames = 8;
	}

	void ClearPendingRagdollImpulse()
	{
		PendingRagdollHitPosition = Vector3.Zero;
		PendingRagdollImpulse = Vector3.Zero;
		PendingRagdollImpulseFrames = 0;
	}

	void ResetDeathState()
	{
		DeathRagdollStarted = false;
		ClearPendingRagdollImpulse();
		DestroyLocalRagdoll();
		SetLiveRenderersVisible( true );

		SetGameplayEnabled( true );
	}
}
