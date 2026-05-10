using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Valtryek Lightning Attack" )]
[Category( "Battle Royale" )]
public sealed class ValtryekLightningAttack : Component
{
	[Property] public float Damage { get; set; } = 42f;
	[Property] public float Cooldown { get; set; } = 0.55f;
	[Property] public float Range { get; set; } = 1200f;
	[Property] public float BranchRadius { get; set; } = 140f;
	[Property] public int MaxBranchTargets { get; set; } = 2;
	[Property] public float BranchDamage { get; set; } = 14f;
	[Property] public float VisualLifetime { get; set; } = 0.22f;
	[Property] public int VisualPointCount { get; set; } = 13;
	[Property] public float VisualChaos { get; set; } = 46f;
	[Property] public float RagdollImpulse { get; set; } = 95f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 24f;
	[Property, Group( "Volt Slide" )] public float VoltSlideCooldown { get; set; } = 8f;
	[Property, Group( "Volt Slide" )] public float VoltSlideDistance { get; set; } = 400f;
	[Property, Group( "Volt Slide" )] public float VoltSlideDuration { get; set; } = 0.24f;
	[Property, Group( "Volt Slide" )] public float VoltSlideTouchRadius { get; set; } = 70f;
	[Property, Group( "Volt Slide" )] public float VoltSlideTouchDamage { get; set; } = 18f;
	[Property, Group( "Volt Slide" )] public float StaticMarkDuration { get; set; } = 4f;
	[Property, Group( "Flash Crash" )] public float FlashCrashCooldown { get; set; } = 11f;
	[Property, Group( "Flash Crash" )] public float FlashCrashRange { get; set; } = 1050f;
	[Property, Group( "Flash Crash" )] public float FlashCrashChargeTime { get; set; } = 1f;
	[Property, Group( "Flash Crash" )] public float FlashCrashProjectileSpeed { get; set; } = 2800f;
	[Property, Group( "Flash Crash" )] public float FlashCrashProjectileRadius { get; set; } = 28f;
	[Property, Group( "Flash Crash" )] public float FlashCrashDamage { get; set; } = 22f;
	[Property, Group( "Flash Crash" )] public float FlashCrashCollapseDelay { get; set; } = 0.25f;
	[Property, Group( "Flash Crash" )] public float FlashCrashStunDuration { get; set; } = 0.35f;
	[Property, Group( "Flash Crash" )] public float FlashCrashStaticStunDuration { get; set; } = 0.75f;
	[Property, Group( "Flash Crash" )] public float FlashCrashStaticChainDamage { get; set; } = 16f;
	[Property, Group( "Flash Crash" )] public float FlashCrashStaticChainRadius { get; set; } = 260f;
	[Property, Group( "Flash Crash" )] public int FlashCrashStaticChainTargets { get; set; } = 2;
	[Property, Group( "Overcharge" )] public float MaxVoltage { get; set; } = 100f;
	[Property, Group( "Overcharge" )] public float HighSpeedVoltagePerSecond { get; set; } = 8f;
	[Property, Group( "Overcharge" )] public float HitVoltageGain { get; set; } = 8f;
	[Property, Group( "Overcharge" )] public float VoltSlideHitVoltageGain { get; set; } = 12f;
	[Property, Group( "Overcharge" )] public float SlideVoltagePerSecond { get; set; } = 5f;
	[Property, Group( "Overcharge" )] public float HighSpeedThreshold { get; set; } = 300f;
	[Property, Group( "Overcharge" )] public float InactiveDelayBeforeDecay { get; set; } = 2.4f;
	[Property, Group( "Overcharge" )] public float VoltageDecayPerSecond { get; set; } = 24f;
	[Property, Group( "Overcharge" )] public float VoltageGainMultiplier { get; set; } = 0.1f;
	[Property, Group( "Overcharge" )] public float VoltageLockoutAfterStormSeconds { get; set; } = 3f;
	[Property, Group( "Overcharge" )] public float StormMoveSpeedMultiplier { get; set; } = 1.16f;
	[Property, Group( "Overcharge" )] public float StormCooldownMultiplier { get; set; } = 0.82f;
	[Property, Group( "Overcharge" )] public float StormStateDuration { get; set; } = 6f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseInterval { get; set; } = 0.58f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseRadius { get; set; } = 180f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseDamage { get; set; } = 12f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseMinDistance { get; set; } = 105f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseMaxDistance { get; set; } = 230f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseStartHeight { get; set; } = 22f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseEndHeight { get; set; } = 5f;
	[Property, Group( "Overcharge" )] public float StormGroundImpulseStartRadius { get; set; } = 18f;
	[Property, Group( "Overcharge" )] public int StormGroundImpulsesPerPulse { get; set; } = 2;
	[Property, Group( "Overclock" )] public float OverclockCooldown { get; set; } = 45f;
	[Property, Group( "Overclock" )] public float OverclockDuration { get; set; } = 7f;
	[Property, Group( "Overclock" )] public float OverheatDuration { get; set; } = 3f;
	[Property, Group( "Overclock" )] public float OverclockMoveSpeedMultiplier { get; set; } = 1.28f;
	[Property, Group( "Overclock" )] public float OverclockCooldownMultiplier { get; set; } = 0.68f;
	[Property, Group( "Overclock" )] public float OverclockVoltageGainMultiplier { get; set; } = 1.45f;
	[Property, Group( "Overclock" )] public float OverheatedMoveSpeedMultiplier { get; set; } = 0.86f;
	[Property, Group( "Overclock" )] public float OverheatedVoltageGainMultiplier { get; set; } = 0.35f;
	[Property, Group( "Muzzle" )] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property, Group( "Muzzle" )] public string StartBoneNames { get; set; } = "finger_index_03_R,finger_index_02_R,finger_index_01_R,index_03_R,index_02_R,index_01_R,hold_R,hand_R";
	[Property, Group( "Muzzle" )] public Vector3 BoneLocalOffset { get; set; } = new( 6f, 0f, 0f );
	[Property, Group( "Muzzle" )] public Vector3 FallbackWorldOffset { get; set; } = new( 30f, 8f, 54f );

	TimeUntil NextAttackTime { get; set; }
	TimeUntil NextAttackRequestTime { get; set; }
	TimeUntil HostNextAttackTime { get; set; }
	TimeUntil NextVoltSlideTime { get; set; }
	TimeUntil HostNextVoltSlideTime { get; set; }
	TimeUntil NextFlashCrashTime { get; set; }
	TimeUntil HostNextFlashCrashTime { get; set; }
	TimeUntil NextOverclockTime { get; set; }
	TimeUntil HostNextOverclockTime { get; set; }
	[Sync( Flags = SyncFlags.FromHost )] public float Voltage { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public bool IsStormState { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public bool IsOverclockActive { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public bool IsOverheated { get; private set; }
	PlayerCombat Combat { get; set; }
	PlayerController Controller { get; set; }
	ValtryekLightningVisual LightningVisual { get; set; }
	PlayerCharacter Character { get; set; }
	bool IsVoltSliding { get; set; }
	Vector3 VoltSlideStartPosition { get; set; }
	Vector3 VoltSlideEndPosition { get; set; }
	TimeSince VoltSlideElapsed { get; set; }
	TimeSince TimeSinceVoltageActivity { get; set; }
	TimeUntil StormStateTime { get; set; }
	TimeUntil OverclockTime { get; set; }
	TimeUntil OverheatTime { get; set; }
	TimeUntil VoltageGainLockoutTime { get; set; }
	TimeUntil NextStormGroundImpulseTime { get; set; }
	TimeSince FlashCrashChargeElapsed { get; set; }
	Vector3 PreviousPosition { get; set; }
	int StormGroundImpulseSeed { get; set; }
	public bool IsFlashCrashCharging { get; private set; }
	HashSet<PlayerCombat> VoltSlideTouchedTargets { get; } = new();
	List<PendingFlashCrashCast> PendingFlashCrashCasts { get; } = new();
	List<PendingFlashCrash> PendingFlashCrashes { get; } = new();

	public float VoltSlideCooldownRemaining => MathF.Max( 0f, NextVoltSlideTime );
	public float VoltSlideCooldownPercent => VoltSlideCooldown <= 0f ? 0f : (VoltSlideCooldownRemaining / VoltSlideCooldown * 100f).Clamp( 0f, 100f );
	public float FlashCrashCooldownRemaining => MathF.Max( 0f, NextFlashCrashTime );
	public float FlashCrashCooldownPercent => FlashCrashCooldown <= 0f ? 0f : (FlashCrashCooldownRemaining / FlashCrashCooldown * 100f).Clamp( 0f, 100f );
	public float OverclockCooldownRemaining => MathF.Max( 0f, NextOverclockTime );
	public float OverclockCooldownPercent => OverclockCooldown <= 0f ? 0f : (OverclockCooldownRemaining / OverclockCooldown * 100f).Clamp( 0f, 100f );
	public float VoltagePercent => MaxVoltage <= 0f ? 0f : (Voltage / MaxVoltage * 100f).Clamp( 0f, 100f );
	public bool CanActivateOvercharge => Voltage >= MaxVoltage && !IsStormState;

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Controller = GetComponent<PlayerController>();
		LightningVisual = GetComponent<ValtryekLightningVisual>();
		Character = GetComponent<PlayerCharacter>();
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		PreviousPosition = WorldPosition;
	}

	protected override void OnFixedUpdate()
	{
		UpdateVoltSlide();
		UpdatePendingFlashCrashCasts();
		UpdatePendingFlashCrashes();
	}

	protected override void OnUpdate()
	{
		UpdateOvercharge();
	}

	public void TryAttack()
	{
		if ( NextAttackTime > 0f || NextAttackRequestTime > 0f )
			return;

		NextAttackRequestTime = 0.04f;
		var origin = GetMuzzlePosition();
		var aimPoint = GetAimPoint( origin );
		PlayAttackVisual();
		RequestFireLightning( origin, aimPoint );
	}

	public void TryVoltSlide()
	{
		if ( NextVoltSlideTime > 0f )
			return;

		var direction = GetVoltSlideDirection();
		if ( direction.Length <= 0.01f )
			return;

		NextVoltSlideTime = GetScaledCooldown( VoltSlideCooldown );
		RequestVoltSlide( direction.Normal );
	}

	public void TryStartFlashCrashCharge()
	{
		if ( NextFlashCrashTime > 0f || IsFlashCrashCharging )
			return;

		NextFlashCrashTime = GetScaledCooldown( FlashCrashCooldown );
		IsFlashCrashCharging = true;
		FlashCrashChargeElapsed = 0f;
		PlayAttackVisual();
		RequestStartFlashCrashCharge();
	}

	public void TryFireFlashCrashCharge()
	{
		if ( !IsFlashCrashCharging )
			return;

		if ( FlashCrashChargeElapsed < FlashCrashChargeTime )
			return;

		IsFlashCrashCharging = false;
		var origin = GetMuzzlePosition();
		var aimPoint = GetAimPoint( origin, FlashCrashRange );
		RequestFireFlashCrashCharge( origin, aimPoint );
	}

	public void TryActivateOvercharge()
	{
		RequestActivateOvercharge();
	}

	public void TryOverclock()
	{
		if ( NextOverclockTime > 0f )
			return;

		RequestOverclock();
	}

	[Rpc.Broadcast]
	void PlayAttackVisual()
	{
		LightningVisual ??= GetComponent<ValtryekLightningVisual>();
		LightningVisual?.PlayAttackVisual();
	}

	[Rpc.Host]
	void RequestFireLightning( Vector3 origin, Vector3 aimPoint )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextAttackTime > 0f )
			return;

		var cooldown = GetScaledCooldown( Cooldown );
		HostNextAttackTime = cooldown;
		ConfirmAttackCooldown( cooldown );

		var clientAimDirection = (aimPoint - origin).Normal;
		origin = GetMuzzlePosition();
		aimPoint = clientAimDirection.Length > 0.01f ? origin + clientAimDirection * Range : GetAimPoint( origin );

		var direction = (aimPoint - origin).Normal;
		if ( direction.Length <= 0.01f )
			direction = WorldRotation.Forward.Normal;

		var traceEnd = origin + direction * Range;
		var trace = Scene.Trace
			.Ray( origin, traceEnd )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "deadplayer" )
			.Run();

		var end = trace.Hit ? trace.HitPosition : traceEnd;
		var mainTarget = GetCombatFromTrace( trace );
		if ( mainTarget.IsValid() && !mainTarget.IsDead )
		{
			var amount = ScaleLightningDamage( Damage );
			var impulse = direction * RagdollImpulse + Vector3.Up * RagdollUpwardImpulse;
			mainTarget.ApplyDamage( new DamageEvent( GameObject, amount, DamageType.Magic, trace.HitPosition, impulse ) );
			AddVoltage( HitVoltageGain );
		}

		var branchStarts = new List<Vector3>();
		var branchEnds = new List<Vector3>();
		ApplyBranches( origin, end, mainTarget, branchStarts, branchEnds );

		BroadcastLightningVisual( origin, end, branchStarts.ToArray(), branchEnds.ToArray(), Game.Random.Next( 0, int.MaxValue ) );
	}

	[Rpc.Owner]
	void ConfirmAttackCooldown( float cooldown )
	{
		NextAttackTime = cooldown;
	}

	[Rpc.Host]
	void RequestVoltSlide( Vector3 direction )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextVoltSlideTime > 0f )
			return;

		var slideDirection = direction.WithZ( 0f ).Normal;
		if ( slideDirection.Length <= 0.01f )
			return;

		HostNextVoltSlideTime = GetScaledCooldown( VoltSlideCooldown );
		StartVoltSlide( slideDirection );
	}

	[Rpc.Host]
	void RequestStartFlashCrashCharge()
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextFlashCrashTime > 0f )
			return;

		var cooldown = GetScaledCooldown( FlashCrashCooldown );
		HostNextFlashCrashTime = cooldown;
		ConfirmFlashCrashCooldown( cooldown );

		BroadcastFlashCrashChargeVisual( GameObject, FlashCrashChargeTime );

		PendingFlashCrashCasts.Add( new PendingFlashCrashCast
		{
			Delay = FlashCrashChargeTime
		} );
	}

	[Rpc.Host]
	void RequestFireFlashCrashCharge( Vector3 origin, Vector3 aimPoint )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( PendingFlashCrashCasts.Count == 0 )
			return;

		var pending = PendingFlashCrashCasts[0];
		PendingFlashCrashCasts.RemoveAt( 0 );

		if ( pending.Delay > 0f )
		{
			PendingFlashCrashCasts.Insert( 0, pending );
			return;
		}

		IsFlashCrashCharging = false;
		var clientAimDirection = (aimPoint - origin).Normal;
		origin = GetMuzzlePosition();
		var direction = clientAimDirection;
		if ( direction.Length <= 0.01f )
			direction = WorldRotation.Forward.Normal;

		StopFlashCrashChargeVisual( GameObject );
		SpawnFlashCrashOrb( direction.Normal );
	}

	[Rpc.Owner]
	void ConfirmFlashCrashCooldown( float cooldown )
	{
		NextFlashCrashTime = cooldown;
	}

	[Rpc.Host]
	void RequestActivateOvercharge()
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( Voltage < MaxVoltage || IsStormState )
			return;

		Voltage = 0f;
		IsStormState = true;
		StormStateTime = StormStateDuration;
		VoltageGainLockoutTime = StormStateDuration + VoltageLockoutAfterStormSeconds;
		NextStormGroundImpulseTime = 0f;
		StormGroundImpulseSeed = Game.Random.Next( 0, int.MaxValue );
		TimeSinceVoltageActivity = 0f;
		TriggerStormGroundImpulse();
	}

	[Rpc.Host]
	void RequestOverclock()
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextOverclockTime > 0f || IsOverclockActive )
			return;

		var cooldown = GetScaledCooldown( OverclockCooldown );
		HostNextOverclockTime = cooldown;
		ConfirmOverclockCooldown( cooldown );

		Voltage = 0f;
		IsOverheated = false;
		IsOverclockActive = true;
		IsStormState = true;
		OverclockTime = OverclockDuration;
		StormStateTime = OverclockDuration;
		VoltageGainLockoutTime = OverclockDuration + VoltageLockoutAfterStormSeconds;
		NextStormGroundImpulseTime = 0f;
		StormGroundImpulseSeed = Game.Random.Next( 0, int.MaxValue );
		TimeSinceVoltageActivity = 0f;
		TriggerStormGroundImpulse();
	}

	[Rpc.Owner]
	void ConfirmOverclockCooldown( float cooldown )
	{
		NextOverclockTime = cooldown;
	}

	void StartVoltSlide( Vector3 direction )
	{
		VoltSlideStartPosition = WorldPosition;
		VoltSlideEndPosition = GetVoltSlideTargetPosition( direction );
		VoltSlideElapsed = 0f;
		VoltSlideTouchedTargets.Clear();
		IsVoltSliding = true;
		AddVoltage( SlideVoltagePerSecond * VoltSlideDuration );
		BroadcastVoltSlideVisual( GameObject, VoltSlideStartPosition, VoltSlideEndPosition, VoltSlideDuration, Game.Random.Next( 0, int.MaxValue ) );
	}

	void UpdateVoltSlide()
	{
		if ( !Networking.IsHost || !IsVoltSliding )
			return;

		var duration = MathF.Max( 0.01f, VoltSlideDuration );
		var progress = (VoltSlideElapsed / duration).Clamp( 0f, 1f );
		var easedProgress = 1f - MathF.Pow( 1f - progress, 3f );
		WorldPosition = Vector3.Lerp( VoltSlideStartPosition, VoltSlideEndPosition, easedProgress );
		ApplyVoltSlideTouchDamage();

		if ( progress >= 1f )
			IsVoltSliding = false;
	}

	void ApplyVoltSlideTouchDamage()
	{
		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target.IsDead || target.GameObject == GameObject || VoltSlideTouchedTargets.Contains( target ) )
				continue;

			var distance = (target.WorldPosition.WithZ( 0f ) - WorldPosition.WithZ( 0f )).Length;
			if ( distance > VoltSlideTouchRadius )
				continue;

			var hitPosition = target.WorldPosition + Vector3.Up * 38f;
			var direction = (target.WorldPosition - WorldPosition).WithZ( 0f ).Normal;
			if ( direction.Length <= 0.01f )
				direction = WorldRotation.Forward.WithZ( 0f ).Normal;

			var damage = IsStormState ? VoltSlideTouchDamage * 1.25f : VoltSlideTouchDamage;
			target.ApplyDamage( new DamageEvent( GameObject, ScaleLightningDamage( damage ), DamageType.Magic, hitPosition, direction * 80f + Vector3.Up * 20f ) );
			target.ApplyStaticMark( StaticMarkDuration );
			VoltSlideTouchedTargets.Add( target );
			AddVoltage( VoltSlideHitVoltageGain );
		}
	}

	void UpdatePendingFlashCrashCasts()
	{
		if ( !Networking.IsHost || PendingFlashCrashCasts.Count == 0 )
			return;

		for ( var i = PendingFlashCrashCasts.Count - 1; i >= 0; i-- )
		{
			var pending = PendingFlashCrashCasts[i];
			pending.Delay -= Time.Delta;
			pending.Delay = MathF.Max( 0f, pending.Delay );
			PendingFlashCrashCasts[i] = pending;
		}
	}

	[Rpc.Owner]
	void ResetFlashCrashCooldown()
	{
		NextFlashCrashTime = 0f;
		IsFlashCrashCharging = false;
	}

	void SpawnFlashCrashOrb( Vector3 direction )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		var origin = GetMuzzlePosition();
		BroadcastSpawnFlashCrashOrb( origin, direction.Normal );
	}

	[Rpc.Broadcast]
	void BroadcastSpawnFlashCrashOrb( Vector3 origin, Vector3 direction )
	{
		var orbObject = new GameObject( true, "Flash Crash Orb" );
		orbObject.NetworkMode = NetworkMode.Never;
		orbObject.WorldPosition = origin;
		orbObject.WorldRotation = Rotation.LookAt( direction, Vector3.Up );
		orbObject.Enabled = true;

		var light = orbObject.Components.Create<PointLight>();
		light.LightColor = new Color( 0.12f, 0.62f, 1f, 0.9f );
		light.Radius = 165f;
		light.Shadows = false;

		var projectile = orbObject.Components.Create<ValtryekFlashCrashOrb>();
		projectile.OwnerAttack = this;
		projectile.Source = GameObject;
		projectile.Direction = direction.Normal;
		projectile.Speed = FlashCrashProjectileSpeed;
		projectile.MaxDistance = FlashCrashRange;
		projectile.HitRadius = FlashCrashProjectileRadius;
	}

	public void NotifyFlashCrashOrbHit( PlayerCombat target, Vector3 hitPosition )
	{
		if ( !Networking.IsHost || !target.IsValid() || target.IsDead )
			return;

		PendingFlashCrashes.Add( new PendingFlashCrash
		{
			Target = target,
			HitPosition = hitPosition,
			Delay = FlashCrashCollapseDelay
		} );
	}

	void UpdatePendingFlashCrashes()
	{
		if ( !Networking.IsHost || PendingFlashCrashes.Count == 0 )
			return;

		for ( var i = PendingFlashCrashes.Count - 1; i >= 0; i-- )
		{
			var pending = PendingFlashCrashes[i];
			pending.Delay -= Time.Delta;
			if ( pending.Delay > 0f )
			{
				PendingFlashCrashes[i] = pending;
				continue;
			}

			ResolveFlashCrash( pending );
			PendingFlashCrashes.RemoveAt( i );
		}
	}

	void ResolveFlashCrash( PendingFlashCrash pending )
	{
		var target = pending.Target;
		if ( !target.IsValid() || target.IsDead )
			return;

		var hadStatic = target.TryConsumeStaticMark( out _ );
		var stunDuration = hadStatic ? FlashCrashStaticStunDuration : FlashCrashStunDuration;
		target.ApplyDamage( new DamageEvent( GameObject, ScaleLightningDamage( FlashCrashDamage ), DamageType.Magic, pending.HitPosition, Vector3.Up * 18f ) );
		target.ApplyStun( stunDuration );
		AddVoltage( HitVoltageGain );
		BroadcastStormGroundImpulseVisual( pending.HitPosition + Vector3.Up * 24f, pending.HitPosition + Vector3.Up * 3f, Game.Random.Next( 0, int.MaxValue ) );

		if ( hadStatic )
			ApplyFlashCrashChain( target, pending.HitPosition );
	}

	void ApplyFlashCrashChain( PlayerCombat sourceTarget, Vector3 origin )
	{
		var candidates = new List<(PlayerCombat Target, float Distance)>();
		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target.IsDead || target == sourceTarget || target.GameObject == GameObject )
				continue;

			var distance = (target.WorldPosition - origin).Length;
			if ( distance > FlashCrashStaticChainRadius )
				continue;

			candidates.Add( (target, distance) );
		}

		candidates.Sort( ( a, b ) => a.Distance.CompareTo( b.Distance ) );
		var count = Math.Min( FlashCrashStaticChainTargets, candidates.Count );
		for ( var i = 0; i < count; i++ )
		{
			var target = candidates[i].Target;
			var hitPosition = target.WorldPosition + Vector3.Up * 42f;
			target.ApplyDamage( new DamageEvent( GameObject, ScaleLightningDamage( FlashCrashStaticChainDamage ), DamageType.Magic, hitPosition ) );
			BroadcastStormGroundImpulseVisual( origin + Vector3.Up * 24f, hitPosition, Game.Random.Next( 0, int.MaxValue ) );
		}
	}

	[Rpc.Broadcast]
	void BroadcastVoltSlideVisual( GameObject sourceObject, Vector3 startPosition, Vector3 endPosition, float duration, int seed )
	{
		if ( !sourceObject.IsValid() )
			return;

		var visualObject = new GameObject( true, "Valtryek Volt Slide Visual" );
		visualObject.NetworkMode = NetworkMode.Never;
		visualObject.WorldPosition = startPosition + Vector3.Up * 28f;
		visualObject.Enabled = true;

		var visual = visualObject.Components.Create<ValtryekVoltSlideVisual>();
		visual.SourceObject = sourceObject;
		visual.StartPosition = startPosition;
		visual.EndPosition = endPosition;
		visual.Duration = duration;
		visual.Seed = seed;
	}

	[Rpc.Broadcast]
	void BroadcastFlashCrashChargeVisual( GameObject sourceObject, float duration )
	{
		if ( !sourceObject.IsValid() )
			return;

		var start = GetMuzzlePosition();
		var end = start + Vector3.Up * 28f + WorldRotation.Right * 12f;
		var visualObject = new GameObject( true, "Flash Crash Charge" );
		visualObject.NetworkMode = NetworkMode.Never;
		visualObject.WorldPosition = start;
		visualObject.Enabled = true;

		var visual = visualObject.Components.Create<ValtryekFlashCrashChargeVisual>();
		visual.SourceObject = sourceObject;
		visual.SourceBoneNames = StartBoneNames;
		visual.SourceBoneLocalOffset = BoneLocalOffset;
		visual.SourceFallbackWorldOffset = FallbackWorldOffset;
		visual.RequiredChargeTime = duration;
		visual.Duration = 30f;
	}

	[Rpc.Broadcast]
	void StopFlashCrashChargeVisual( GameObject sourceObject )
	{
		foreach ( var visual in Scene.GetAllComponents<ValtryekFlashCrashChargeVisual>() )
		{
			if ( visual.SourceObject == sourceObject )
				visual.GameObject.Destroy();
		}
	}

	[Rpc.Broadcast]
	void BroadcastStormGroundImpulseVisual( Vector3 start, Vector3 end, int seed )
	{
		var visualObject = new GameObject( true, "Valtryek Storm Ground Impulse" );
		visualObject.NetworkMode = NetworkMode.Never;
		visualObject.WorldPosition = start;
		visualObject.Enabled = true;

		var visual = visualObject.Components.Create<ValtryekGroundLightningVisual>();
		visual.Start = start;
		visual.End = end;
		visual.Seed = seed;
		visual.Lifetime = 0.18f;
		visual.CoreScale = 0.42f;
		visual.GlowScale = 1.28f;
		visual.PointCount = 5;
		visual.Chaos = 22f;
		visual.BranchChance = 0.35f;
		visual.SecondaryBoltChance = 0.35f;
	}

	[Rpc.Broadcast]
	void BroadcastLightningVisual( Vector3 start, Vector3 end, Vector3[] branchStarts, Vector3[] branchEnds, int seed )
	{
		var visualObject = new GameObject( true, "Valtryek Lightning Visual" );
		visualObject.NetworkMode = NetworkMode.Never;
		visualObject.WorldPosition = start;
		visualObject.Enabled = true;

		var visual = visualObject.Components.Create<LightningBoltVisual>();
		visual.Start = start;
		visual.End = end;
		visual.SourceObject = GameObject;
		visual.SourceBoneNames = StartBoneNames;
		visual.SourceBoneLocalOffset = BoneLocalOffset;
		visual.SourceFallbackWorldOffset = FallbackWorldOffset;
		visual.Seed = seed;
		visual.PointCount = VisualPointCount;
		visual.Chaos = VisualChaos;
		visual.Lifetime = VisualLifetime;
		visual.BranchStarts = new List<Vector3>( branchStarts ?? Array.Empty<Vector3>() );
		visual.BranchEnds = new List<Vector3>( branchEnds ?? Array.Empty<Vector3>() );
	}

	void ApplyBranches( Vector3 start, Vector3 end, PlayerCombat mainTarget, List<Vector3> branchStarts, List<Vector3> branchEnds )
	{
		var candidates = new List<(PlayerCombat Target, Vector3 BranchStart, float Distance)>();

		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !IsValidBranchTarget( target, mainTarget ) )
				continue;

			var targetPoint = target.WorldPosition + Vector3.Up * 42f;
			var closest = GetClosestPointOnSegment( start, end, targetPoint );
			var distance = (targetPoint - closest).Length;
			if ( distance > BranchRadius )
				continue;

			candidates.Add( (target, closest, distance) );
		}

		candidates.Sort( ( a, b ) => a.Distance.CompareTo( b.Distance ) );
		var maxBranchTargets = GetCurrentMaxBranchTargets();
		var count = Math.Min( maxBranchTargets, candidates.Count );
		for ( var i = 0; i < count; i++ )
		{
			var candidate = candidates[i];
			var hitPosition = candidate.Target.WorldPosition + Vector3.Up * 42f;
			var direction = (hitPosition - candidate.BranchStart).Normal;
			if ( direction.Length <= 0.01f )
				direction = (end - start).Normal;

			var impulse = direction * (RagdollImpulse * 0.45f) + Vector3.Up * (RagdollUpwardImpulse * 0.4f);
			candidate.Target.ApplyDamage( new DamageEvent( GameObject, ScaleLightningDamage( GetCurrentBranchDamage() ), DamageType.Magic, hitPosition, impulse ) );
			branchStarts.Add( candidate.BranchStart );
			branchEnds.Add( hitPosition );
			AddVoltage( HitVoltageGain * 0.5f );
		}
	}

	bool IsValidBranchTarget( PlayerCombat target, PlayerCombat mainTarget )
	{
		if ( !target.IsValid() || target.IsDead )
			return false;

		if ( target == mainTarget )
			return false;

		if ( target.GameObject == GameObject )
			return false;

		return true;
	}

	float ScaleLightningDamage( float baseDamage )
	{
		return Combat.IsValid() ? Combat.ScaleDamage( baseDamage, DamageType.Magic ) : baseDamage;
	}

	float GetScaledCooldown( float cooldown )
	{
		var scaledCooldown = Combat.IsValid() ? Combat.ScaleCooldown( cooldown ) : cooldown;
		if ( IsOverclockActive )
			return scaledCooldown * OverclockCooldownMultiplier;

		return IsStormState ? scaledCooldown * StormCooldownMultiplier : scaledCooldown;
	}

	int GetCurrentMaxBranchTargets()
	{
		if ( IsStormState )
			return IsOverclockActive ? MaxBranchTargets + 3 : MaxBranchTargets + 2;

		return MaxBranchTargets;
	}

	float GetCurrentBranchDamage()
	{
		if ( IsStormState )
			return IsOverclockActive ? BranchDamage * 2f : BranchDamage * 1.65f;

		return BranchDamage;
	}

	void UpdateOvercharge()
	{
		if ( !Networking.IsHost || !IsValtryekSelected() || (Combat.IsValid() && Combat.IsDead) )
		{
			PreviousPosition = WorldPosition;
			return;
		}

		var movement = (WorldPosition - PreviousPosition).WithZ( 0f );
		var speed = Time.Delta > 0f ? movement.Length / Time.Delta : 0f;
		if ( speed >= HighSpeedThreshold )
			AddVoltage( HighSpeedVoltagePerSecond * Time.Delta );

		UpdateStormState();
		UpdateOverclockState();
		ApplyOverchargeMoveSpeed();
		UpdateStormGroundImpulses();
		PreviousPosition = WorldPosition;
	}

	void UpdateStormState()
	{
		if ( !IsStormState )
			return;

		if ( StormStateTime > 0f )
			return;

		IsStormState = false;
	}

	void UpdateOverclockState()
	{
		if ( IsOverclockActive && OverclockTime <= 0f )
		{
			IsOverclockActive = false;
			IsOverheated = true;
			OverheatTime = OverheatDuration;
			IsStormState = false;
		}

		if ( IsOverheated && OverheatTime <= 0f )
			IsOverheated = false;
	}

	void ApplyOverchargeMoveSpeed()
	{
		if ( !Combat.IsValid() )
			return;

		if ( IsOverclockActive )
		{
			Combat.ApplyMoveSpeedMultiplierEffect( OverclockMoveSpeedMultiplier, 0.25f );
			return;
		}

		if ( IsStormState )
		{
			Combat.ApplyMoveSpeedMultiplierEffect( StormMoveSpeedMultiplier, 0.25f );
			return;
		}

		if ( IsOverheated )
			Combat.ApplyMoveSpeedMultiplierEffect( OverheatedMoveSpeedMultiplier, 0.25f );
	}

	void UpdateStormGroundImpulses()
	{
		if ( !IsStormState || NextStormGroundImpulseTime > 0f )
			return;

		NextStormGroundImpulseTime = StormGroundImpulseInterval;
		TriggerStormGroundImpulse();
	}

	void TriggerStormGroundImpulse()
	{
		var groundCenter = GetStormGroundPoint( WorldPosition );
		var count = StormGroundImpulsesPerPulse.Clamp( 1, 8 );
		var baseAngle = (StormGroundImpulseSeed++ % count) * (360f / count) * 0.5f;

		for ( var i = 0; i < count; i++ )
		{
			var angle = (baseAngle + i * (360f / count)) * MathF.PI / 180f;
			var distance = Game.Random.Float( StormGroundImpulseMinDistance, StormGroundImpulseMaxDistance );
			var direction = new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0f ).Normal;
			var start = groundCenter + direction * Game.Random.Float( StormGroundImpulseStartRadius * 0.45f, StormGroundImpulseStartRadius * 1.25f ) + Vector3.Up * Game.Random.Float( StormGroundImpulseStartHeight * 0.65f, StormGroundImpulseStartHeight * 1.35f );
			var end = GetStormGroundPoint( WorldPosition + direction * distance ) + Vector3.Up * StormGroundImpulseEndHeight;

			BroadcastStormGroundImpulseVisual( start, end, Game.Random.Next( 0, int.MaxValue ) );
			ApplyStormGroundImpulseDamage( end );
		}
	}

	Vector3 GetStormGroundPoint( Vector3 position )
	{
		var trace = Scene.Trace
			.Ray( position + Vector3.Up * 90f, position + Vector3.Down * 220f )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "deadplayer" )
			.Run();

		return trace.Hit ? trace.HitPosition + Vector3.Up * 4f : position;
	}

	void ApplyStormGroundImpulseDamage( Vector3 impactPosition )
	{
		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target.IsDead || target.GameObject == GameObject )
				continue;

			var distance = (target.WorldPosition.WithZ( 0f ) - impactPosition.WithZ( 0f )).Length;
			if ( distance > StormGroundImpulseRadius )
				continue;

			var hitPosition = target.WorldPosition + Vector3.Up * 36f;
			var direction = (target.WorldPosition - impactPosition).WithZ( 0f ).Normal;
			if ( direction.Length <= 0.01f )
				direction = (target.WorldPosition - WorldPosition).WithZ( 0f ).Normal;

			target.ApplyDamage( new DamageEvent( GameObject, ScaleLightningDamage( StormGroundImpulseDamage ), DamageType.Magic, hitPosition, direction * 55f + Vector3.Up * 18f ) );
		}
	}

	void AddVoltage( float amount )
	{
		if ( !Networking.IsHost || amount <= 0f )
			return;

		if ( VoltageGainLockoutTime > 0f || IsStormState || IsOverclockActive )
			return;

		amount *= VoltageGainMultiplier;

		if ( IsOverheated )
			amount *= OverheatedVoltageGainMultiplier;
		else if ( IsOverclockActive )
			amount *= OverclockVoltageGainMultiplier;

		SetVoltage( Voltage + amount );
		TimeSinceVoltageActivity = 0f;
	}

	void SetVoltage( float voltage )
	{
		Voltage = voltage.Clamp( 0f, MaxVoltage );
	}

	bool IsValtryekSelected()
	{
		Character ??= GetComponent<PlayerCharacter>();
		return Character.IsValid() && Character.CurrentCharacter == CharacterId.Valtryek;
	}

	Vector3 GetMuzzlePosition()
	{
		var startBone = GetStartBoneObject();
		if ( startBone.IsValid() )
			return startBone.WorldPosition + startBone.WorldRotation * BoneLocalOffset;

		return WorldPosition
			+ WorldRotation.Forward * FallbackWorldOffset.x
			+ WorldRotation.Right * FallbackWorldOffset.y
			+ Vector3.Up * FallbackWorldOffset.z;
	}

	Vector3 GetVoltSlideDirection()
	{
		var camera = GetMainCamera();
		var forward = camera.IsValid() ? camera.WorldRotation.Forward.WithZ( 0f ) : WorldRotation.Forward.WithZ( 0f );
		var right = camera.IsValid() ? camera.WorldRotation.Right.WithZ( 0f ) : WorldRotation.Right.WithZ( 0f );
		if ( forward.Length <= 0.01f )
			forward = WorldRotation.Forward.WithZ( 0f );

		if ( forward.Length <= 0.01f )
			forward = Vector3.Forward;

		if ( right.Length <= 0.01f )
			right = Vector3.Right;

		var direction = Vector3.Zero;
		if ( Input.Down( "Forward" ) )
			direction += forward.Normal;
		if ( Input.Down( "Backward" ) )
			direction -= forward.Normal;
		if ( Input.Down( "Left" ) )
			direction -= right.Normal;
		if ( Input.Down( "Right" ) )
			direction += right.Normal;

		return direction.Length > 0.01f ? direction.Normal : forward.Normal;
	}

	Vector3 GetVoltSlideTargetPosition( Vector3 slideDirection )
	{
		var start = WorldPosition + Vector3.Up * 18f;
		var end = start + slideDirection * VoltSlideDistance;
		var trace = Scene.Trace
			.Ray( start, end )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "deadplayer" )
			.Run();

		var target = trace.Hit ? trace.HitPosition - slideDirection * 20f : end;
		return new Vector3( target.x, target.y, WorldPosition.z );
	}

	GameObject GetStartBoneObject()
	{
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		if ( !BodyRenderer.IsValid() || string.IsNullOrWhiteSpace( StartBoneNames ) )
			return null;

		var names = StartBoneNames.Split( new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries );
		foreach ( var rawName in names )
		{
			var boneName = rawName.Trim();
			if ( string.IsNullOrWhiteSpace( boneName ) )
				continue;

			var boneObject = BodyRenderer.GetBoneObject( boneName );
			if ( boneObject.IsValid() )
				return boneObject;
		}

		return null;
	}

	Vector3 GetAimPoint( Vector3 origin, float aimRange = -1f )
	{
		aimRange = aimRange > 0f ? aimRange : Range;
		var camera = GetMainCamera();
		if ( camera.IsValid() )
		{
			var start = camera.WorldPosition;
			var end = start + camera.WorldRotation.Forward * aimRange;
			var trace = Scene.Trace
				.Ray( start, end )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "trigger", "deadplayer" )
				.Run();

			return trace.Hit ? trace.HitPosition : end;
		}

		if ( Controller.IsValid() )
			return origin + Controller.EyeAngles.Forward.Normal * aimRange;

		return origin + WorldRotation.Forward.Normal * aimRange;
	}

	PlayerCombat GetCombatFromTrace( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return null;

		var hitObject = trace.Collider?.GameObject ?? trace.GameObject;
		if ( !hitObject.IsValid() )
			return null;

		return hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
	}

	CameraComponent GetMainCamera()
	{
		foreach ( var camera in Scene.GetAllComponents<CameraComponent>() )
		{
			if ( camera.IsValid() && camera.IsMainCamera )
				return camera;
		}

		return null;
	}

	static Vector3 GetClosestPointOnSegment( Vector3 start, Vector3 end, Vector3 point )
	{
		var segment = end - start;
		var lengthSquared = Vector3.Dot( segment, segment );
		if ( lengthSquared <= 0.01f )
			return start;

		var t = Vector3.Dot( point - start, segment ) / lengthSquared;
		t = t.Clamp( 0f, 1f );
		return start + segment * t;
	}

	struct PendingFlashCrash
	{
		public PlayerCombat Target;
		public Vector3 HitPosition;
		public float Delay;
	}

	struct PendingFlashCrashCast
	{
		public float Delay;
	}
}
