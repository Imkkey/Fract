using System;

namespace Sandbox;

[Title( "Card Attack" )]
[Category( "Battle Royale" )]
public sealed class CardAttack : Component
{
	[Property] public float Damage { get; set; } = 18f;
	[Property] public float Range { get; set; } = 900f;
	[Property] public float Cooldown { get; set; } = 0.65f;
	[Property] public float SpreadDegrees { get; set; } = 6f;
	[Property] public float VerticalCardSpacing { get; set; } = 8f;
	[Property] public float RagdollImpulse { get; set; } = 180f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 45f;
	[Property] public Model CardModel { get; set; }
	[Property] public float CardVisualSpeed { get; set; } = 1800f;

	TimeUntil NextAttackTime { get; set; }

	PlayerCombat Combat { get; set; }
	PlayerController Controller { get; set; }
	CardThrowVisual ThrowVisual { get; set; }

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
		Rotation rotation = WorldRotation;
		if ( Controller.IsValid() )
		{
			rotation = Controller.EyeAngles;
		}

		var forward = rotation.Forward.Normal;
		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( Cooldown ) : Cooldown;

		NextAttackTime = cooldown;
		PlayThrowVisual();
		RequestThrowCards( origin, forward );
	}

	[Rpc.Broadcast]
	void PlayThrowVisual()
	{
		ThrowVisual ??= GetComponent<CardThrowVisual>();
		ThrowVisual?.PlayThrowVisual();
	}

	[Rpc.Host]
	void RequestThrowCards( Vector3 origin, Vector3 forward )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		for ( var i = 0; i < 3; i++ )
		{
			var yaw = (i - 1) * SpreadDegrees;
			var direction = Rotation.FromYaw( yaw ) * forward;
			var cardOrigin = origin + Vector3.Up * ((1 - i) * VerticalCardSpacing);
			var visualDistance = TracePiercingCard( cardOrigin, direction.Normal );
			SpawnCardVisual( cardOrigin, direction.Normal, visualDistance );
		}
	}

	[Rpc.Broadcast]
	void SpawnCardVisual( Vector3 origin, Vector3 direction, float distance )
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
		projectile.MaxDistance = distance.Clamp( 1f, Range );
		projectile.Speed = CardVisualSpeed;
	}

	float TracePiercingCard( Vector3 origin, Vector3 direction )
	{
		var end = origin + direction * Range;
		var hits = Scene.Trace
			.Ray( origin, end )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "deadplayer" )
			.RunAll();

		var damagedPlayers = new HashSet<PlayerCombat>();

		foreach ( var hit in hits )
		{
			if ( !hit.Hit )
				continue;

			var hitObject = hit.Collider?.GameObject ?? hit.GameObject;
			if ( !hitObject.IsValid() )
				continue;

			if ( TryFindCombat( hitObject, out var target ) )
			{
				if ( target == Combat || !damagedPlayers.Add( target ) )
					continue;

				var amount = Combat.IsValid() ? Combat.ScaleDamage( Damage, DamageType.Physical ) : Damage;
				var impulse = direction.Normal * RagdollImpulse + Vector3.Up * RagdollUpwardImpulse;
				target.ApplyDamage( new DamageEvent( GameObject, amount, DamageType.Physical, hit.HitPosition, impulse ) );
				continue;
			}

			return MathF.Max( 1f, (hit.HitPosition - origin).Length );
		}

		return Range;
	}

	static bool TryFindCombat( GameObject hitObject, out PlayerCombat combat )
	{
		combat = hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
		return combat.IsValid();
	}
}
