using System;

namespace Sandbox;

[Title( "Card Attack" )]
[Category( "Battle Royale" )]
public sealed class CardAttack : Component
{
	[Property] public float Damage { get; set; } = 38f;
	[Property] public float Range { get; set; } = 1024f;
	[Property] public float Cooldown { get; set; } = 0.65f;
	[Property] public float SpreadDegrees { get; set; } = 2f;
	[Property] public float VerticalCardSpacing { get; set; } = 2f;
	[Property] public float RagdollImpulse { get; set; } = 180f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 45f;
	[Property] public Model CardModel { get; set; }
	[Property] public float CardVisualSpeed { get; set; } = 1654f;
	[Property] public int MaxPiercedEnemies { get; set; } = 2;
	[Property] public float DamageFalloffPerPierce { get; set; } = 0.35f;
	[Property, Group( "Aim" )] public float AimTraceRange { get; set; } = 5000f;
	[Property, Group( "Skill Stub" )] public float SkillStubBaseDamage { get; set; } = 30f;

	TimeUntil NextAttackTime { get; set; }

	PlayerCombat Combat { get; set; }
	PlayerController Controller { get; set; }
	CardThrowVisual ThrowVisual { get; set; }
	int NextAttackId { get; set; }
	Dictionary<int, Dictionary<PlayerCombat, int>> AttackCardHits { get; } = new();

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Controller = GetComponent<PlayerController>();
		ThrowVisual = GetComponent<CardThrowVisual>();
		CardModel ??= Model.Load( "models/card.vmdl" );
	}

	public void TryAttack()
	{
		if ( NextAttackTime > 0f )
			return;

		var origin = WorldPosition + Vector3.Up * 52f + WorldRotation.Forward * 32f + WorldRotation.Right * 10f;
		var aimPoint = GetAimPoint( origin );
		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( Cooldown ) : Cooldown;

		NextAttackTime = cooldown;
		PlayThrowVisual();
		RequestThrowCards( origin, aimPoint );
	}

	[Rpc.Broadcast]
	void PlayThrowVisual()
	{
		ThrowVisual ??= GetComponent<CardThrowVisual>();
		ThrowVisual?.PlayThrowVisual();
	}

	[Rpc.Host]
	void RequestThrowCards( Vector3 origin, Vector3 aimPoint )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		var forward = (aimPoint - origin).Normal;
		if ( forward.Length <= 0.01f )
			forward = WorldRotation.Forward.Normal;

		var attackId = NextAttackId++;
		AttackCardHits[attackId] = new Dictionary<PlayerCombat, int>();

		for ( var i = 0; i < 3; i++ )
		{
			var yaw = (i - 1) * SpreadDegrees;
			var direction = Rotation.FromYaw( yaw ) * forward;
			var cardOrigin = origin + Vector3.Up * ((1 - i) * VerticalCardSpacing);
			SpawnCardProjectile( cardOrigin, direction.Normal, attackId );
		}
	}

	[Rpc.Broadcast]
	void SpawnCardProjectile( Vector3 origin, Vector3 direction, int attackId )
	{
		CardModel ??= Model.Load( "models/card.vmdl" );
		if ( !CardModel.IsValid() )
			return;

		var cardObject = new GameObject( true, "Card Projectile Visual" );
		cardObject.NetworkMode = NetworkMode.Never;
		cardObject.WorldPosition = origin;
		cardObject.WorldRotation = Rotation.LookAt( direction.Normal, Vector3.Up );

		var renderer = cardObject.Components.Create<ModelRenderer>();
		renderer.Model = CardModel;

		var projectile = cardObject.Components.Create<CardProjectileVisual>();
		projectile.Direction = direction.Normal;
		projectile.MaxDistance = Range;
		projectile.Speed = CardVisualSpeed;
		projectile.Source = GameObject;
		projectile.OwnerAttack = this;
		projectile.AttackId = attackId;
		projectile.Damage = Damage;
		projectile.DamageTypeAfterPierce = DamageType.Magic;
		projectile.InitialDamageType = DamageType.Physical;
		projectile.RagdollImpulse = RagdollImpulse;
		projectile.RagdollUpwardImpulse = RagdollUpwardImpulse;
		projectile.MaxPiercedEnemies = MaxPiercedEnemies;
		projectile.DamageFalloffPerPierce = DamageFalloffPerPierce;
	}

	public void ApplySkillStubDamage( PlayerCombat target, Vector3 hitPosition )
	{
		if ( !Networking.IsHost || !target.IsValid() || target.IsDead )
			return;

		var amount = target.ApplyCardveilDamageBonus( SkillStubBaseDamage );
		if ( target.TryTriggerLuckyCut( out var luckyCutDamage ) )
		{
			amount += luckyCutDamage;
		}

		target.ApplyDamage( new DamageEvent( GameObject, amount, DamageType.Magic, hitPosition ) );
	}

	public void RegisterProjectileHit( int attackId, PlayerCombat target )
	{
		if ( !Networking.IsHost || !target.IsValid() )
			return;

		if ( !AttackCardHits.TryGetValue( attackId, out var targetCardHits ) )
		{
			targetCardHits = new Dictionary<PlayerCombat, int>();
			AttackCardHits[attackId] = targetCardHits;
		}

		AddTargetCardHit( targetCardHits, target );
		if ( targetCardHits[target] is 2 or 3 )
			target.ApplyBleedLuckStacks( 1 );
	}

	public float ScaleCardDamage( float baseDamage, DamageType damageType )
	{
		return Combat.IsValid() ? Combat.ScaleDamage( baseDamage, damageType ) : baseDamage;
	}

	Vector3 GetAimPoint( Vector3 cardOrigin )
	{
		var camera = GetMainCamera();
		if ( camera.IsValid() )
		{
			var start = camera.WorldPosition;
			var end = start + camera.WorldRotation.Forward * AimTraceRange;
			var trace = Scene.Trace
				.Ray( start, end )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "trigger", "deadplayer" )
				.Run();

			return trace.Hit ? trace.HitPosition : end;
		}

		if ( Controller.IsValid() )
			return cardOrigin + Controller.EyeAngles.Forward.Normal * AimTraceRange;

		return cardOrigin + WorldRotation.Forward.Normal * AimTraceRange;
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

	static void AddTargetCardHit( Dictionary<PlayerCombat, int> targetCardHits, PlayerCombat target )
	{
		if ( !targetCardHits.TryAdd( target, 1 ) )
		{
			targetCardHits[target]++;
		}
	}

	static bool TryFindCombat( GameObject hitObject, out PlayerCombat combat )
	{
		combat = hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
		return combat.IsValid();
	}
}
