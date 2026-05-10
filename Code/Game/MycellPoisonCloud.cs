using System.Collections.Generic;

namespace Sandbox;

[Title( "Mycell Poison Cloud" )]
[Category( "Battle Royale" )]
public sealed class MycellPoisonCloud : Component
{
	public GameObject Source { get; set; }
	public MycellAttack OwnerAttack { get; set; }
	public float Radius { get; set; } = 86f;
	public float Duration { get; set; } = 3.2f;
	public float TickInterval { get; set; } = 0.5f;
	public float TickDamage { get; set; } = 8f;
	public float MoveSpeedMultiplier { get; set; } = 0.92f;
	public float SlowDuration { get; set; } = 0.65f;
	public float OwnerMoveSpeedMultiplier { get; set; } = 1.08f;
	public float OwnerBuffDuration { get; set; } = 0.65f;
	public string FogPrefabPath { get; set; } = "prefabs/shroomfog.prefab";
	public Vector3 FogLocalOffset { get; set; } = new( 0f, 0f, 4f );
	public float FogFadeOutSeconds { get; set; } = 0.85f;
	public Color DebugColor { get; set; } = new( 0.54f, 1f, 0.22f, 1f );

	TimeUntil LifeTime { get; set; }
	TimeUntil NextTickTime { get; set; }
	GameObject FogObject { get; set; }
	Vector3 FogStartScale { get; set; } = Vector3.One;
	public bool HasDamagedEnemy { get; private set; }

	protected override void OnStart()
	{
		LifeTime = Duration;
		NextTickTime = 0f;
		SpawnFogVisual();
	}

	protected override void OnUpdate()
	{
		UpdateFogFade();

		if ( LifeTime <= 0f )
		{
			GameObject.Destroy();
			return;
		}

		if ( Networking.IsHost && NextTickTime <= 0f )
		{
			NextTickTime = TickInterval;
			DamageTargetsInsideCloud();
		}
	}

	void DamageTargetsInsideCloud()
	{
		var damagedTargets = new HashSet<PlayerCombat>();
		ApplyOwnerSpeedBuffIfInsideCloud();

		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target.IsDead || target.GameObject == Source )
				continue;

			var toTarget = target.WorldPosition + Vector3.Up * 36f - WorldPosition;
			if ( toTarget.Length > Radius )
				continue;

			if ( !damagedTargets.Add( target ) )
				continue;

			var amount = OwnerAttack.IsValid() ? OwnerAttack.ScaleMycellDamage( TickDamage ) : TickDamage;
			target.ApplyDamage( new DamageEvent( Source, amount, DamageType.Magic, target.WorldPosition + Vector3.Up * 36f ) );
			target.ApplyMoveSlow( MoveSpeedMultiplier, SlowDuration );
			HasDamagedEnemy = true;
		}
	}

	public void Consume()
	{
		GameObject.Destroy();
	}

	void ApplyOwnerSpeedBuffIfInsideCloud()
	{
		if ( !Source.IsValid() )
			return;

		var ownerCombat = Source.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InDescendants );
		if ( !ownerCombat.IsValid() || ownerCombat.IsDead )
			return;

		var toOwner = ownerCombat.WorldPosition + Vector3.Up * 36f - WorldPosition;
		if ( toOwner.Length > Radius )
			return;

		ownerCombat.ApplyMoveSpeedMultiplierEffect( OwnerMoveSpeedMultiplier, OwnerBuffDuration );
	}

	void SpawnFogVisual()
	{
		if ( string.IsNullOrWhiteSpace( FogPrefabPath ) )
			return;

		FogObject = GameObject.Clone( FogPrefabPath, WorldTransform, GameObject, false, "Mycell Poison Fog Visual" );
		if ( FogObject.IsValid() )
		{
			FogObject.NetworkMode = NetworkMode.Never;
			FogObject.Enabled = true;
			FogObject.LocalPosition = FogLocalOffset;
			FogStartScale = FogObject.LocalScale;
		}
	}

	void UpdateFogFade()
	{
		if ( !FogObject.IsValid() || FogFadeOutSeconds <= 0f )
			return;

		var fade = (LifeTime / FogFadeOutSeconds).Clamp( 0f, 1f );
		var easedFade = fade * fade * (3f - 2f * fade);
		FogObject.LocalScale = FogStartScale * easedFade;
	}

	protected override void OnDestroy()
	{
		if ( FogObject.IsValid() )
			FogObject.Destroy();
	}
}
