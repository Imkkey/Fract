using System;

namespace Sandbox;

[Title( "Card Projectile Visual" )]
[Category( "Battle Royale" )]
public sealed class CardProjectileVisual : Component
{
	[Property] public float Speed { get; set; } = 1654f;
	[Property] public Angles ModelFlightAngles { get; set; } = new( 0f, 0f, 0f );
	[Property] public float Damage { get; set; } = 38f;
	[Property] public DamageType InitialDamageType { get; set; } = DamageType.Physical;
	[Property] public DamageType DamageTypeAfterPierce { get; set; } = DamageType.Magic;
	[Property] public float RagdollImpulse { get; set; } = 180f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 45f;
	[Property] public int MaxPiercedEnemies { get; set; } = 2;
	[Property] public float DamageFalloffPerPierce { get; set; } = 0.35f;
	[Property] public float PhysicalDamageMultiplier { get; set; } = 1f;

	public Vector3 Direction { get; set; } = Vector3.Forward;
	public float MaxDistance { get; set; } = 900f;
	public GameObject Source { get; set; }
	public CardAttack OwnerAttack { get; set; }
	public int AttackId { get; set; }
	public bool IsMarkedDeckCard { get; set; }
	public LoadedHandCard LoadedHandCard { get; set; } = LoadedHandCard.Eye;

	float TravelledDistance { get; set; }
	int PiercedPlayers { get; set; }
	HashSet<PlayerCombat> DamagedPlayers { get; } = new();

	protected override void OnUpdate()
	{
		var direction = Direction.Length > 0.01f ? Direction.Normal : Vector3.Forward;
		var step = Speed * Time.Delta;
		var previousPosition = WorldPosition;
		var nextPosition = WorldPosition + direction * step;

		if ( Networking.IsHost && HandleProjectileTouch( previousPosition, nextPosition, direction ) )
			return;

		TravelledDistance += step;
		WorldPosition = nextPosition;

		var up = MathF.Abs( direction.Dot( Vector3.Up ) ) > 0.98f ? Vector3.Right : Vector3.Up;
		WorldRotation = Rotation.LookAt( direction, up ) * ModelFlightAngles.ToRotation();

		if ( TravelledDistance >= MaxDistance )
		{
			GameObject.Destroy();
		}
	}

	bool HandleProjectileTouch( Vector3 previousPosition, Vector3 nextPosition, Vector3 direction )
	{
		var hits = Scene.Trace
			.Ray( previousPosition, nextPosition )
			.IgnoreGameObjectHierarchy( Source )
			.WithoutTags( "trigger", "deadplayer" )
			.RunAll();

		foreach ( var hit in hits )
		{
			if ( !hit.Hit )
				continue;

			var hitObject = hit.Collider?.GameObject ?? hit.GameObject;
			if ( !hitObject.IsValid() )
				continue;

			if ( TryFindCombat( hitObject, out var target ) )
			{
				TouchPlayer( target, hit.HitPosition, direction );
				continue;
			}

			WorldPosition = hit.HitPosition;
			GameObject.Destroy();
			return true;
		}

		return false;
	}

	void TouchPlayer( PlayerCombat target, Vector3 hitPosition, Vector3 direction )
	{
		if ( !target.IsValid() || target.IsDead || !DamagedPlayers.Add( target ) )
			return;

		var damageType = PiercedPlayers == 0 ? InitialDamageType : DamageTypeAfterPierce;
		var falloff = MathF.Pow( 1f - DamageFalloffPerPierce.Clamp( 0f, 0.95f ), PiercedPlayers );
		var damageMultiplier = damageType == DamageType.Physical ? PhysicalDamageMultiplier : 1f;
		var amount = OwnerAttack.IsValid() ? OwnerAttack.ScaleCardDamage( Damage * falloff * damageMultiplier, damageType ) : Damage * falloff * damageMultiplier;
		amount = target.ApplyCardveilDamageBonus( amount );
		var impulse = direction.Normal * RagdollImpulse + Vector3.Up * RagdollUpwardImpulse;

		target.ApplyDamage( new DamageEvent( Source, amount, damageType, hitPosition, impulse ) );
		OwnerAttack?.RegisterProjectileHit( AttackId, target );
		if ( IsMarkedDeckCard )
			OwnerAttack?.NotifyMarkedDeckHit( target, LoadedHandCard );

		PiercedPlayers++;

		if ( PiercedPlayers > MaxPiercedEnemies )
		{
			WorldPosition = hitPosition;
			GameObject.Destroy();
		}
	}

	static bool TryFindCombat( GameObject hitObject, out PlayerCombat combat )
	{
		combat = hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
		return combat.IsValid();
	}
}
