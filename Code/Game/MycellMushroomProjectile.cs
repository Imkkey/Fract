namespace Sandbox;

[Title( "Mycell Mushroom Projectile" )]
[Category( "Battle Royale" )]
public sealed class MycellMushroomProjectile : Component
{
	public Vector3 Direction { get; set; }
	public float Speed { get; set; } = 900f;
	public float UpwardVelocity { get; set; } = 120f;
	public float Gravity { get; set; } = 780f;
	public float MaxDistance { get; set; } = 980f;
	public float MaxLifetime { get; set; } = 6f;
	public float HitRadius { get; set; } = 18f;
	public GameObject Source { get; set; }
	public MycellAttack OwnerAttack { get; set; }
	public float Damage { get; set; } = 28f;
	public float RagdollImpulse { get; set; } = 110f;
	public float RagdollUpwardImpulse { get; set; } = 35f;
	public float CloudRadius { get; set; } = 86f;
	public float CloudDuration { get; set; } = 3.2f;
	public float CloudTickInterval { get; set; } = 0.5f;
	public float CloudTickDamage { get; set; } = 8f;
	public float CloudMoveSpeedMultiplier { get; set; } = 0.92f;
	public float CloudSlowDuration { get; set; } = 0.65f;
	public float CloudOwnerMoveSpeedMultiplier { get; set; } = 1.08f;
	public float CloudOwnerBuffDuration { get; set; } = 0.65f;
	public string CloudFogPrefabPath { get; set; } = "prefabs/shroomfog.prefab";
	public Color DebugColor { get; set; } = new( 0.54f, 1f, 0.22f, 1f );

	float TravelledDistance { get; set; }
	Vector3 Velocity { get; set; }
	TimeUntil LifeTime { get; set; }

	protected override void OnStart()
	{
		if ( Direction.Length <= 0.01f )
			Direction = WorldRotation.Forward.Normal;

		Velocity = Direction.Normal * Speed + Vector3.Up * UpwardVelocity;
		LifeTime = MaxLifetime;
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f )
		{
			GameObject.Destroy();
			return;
		}

		var previous = WorldPosition;
		Velocity += Vector3.Down * Gravity * Time.Delta;
		var movement = Velocity * Time.Delta;
		var next = previous + movement;

		if ( HandleTouch( previous, next ) )
			return;

		WorldPosition = next;
		if ( Velocity.Length > 0.01f )
			WorldRotation = Rotation.LookAt( Velocity.Normal, Vector3.Up ) * Rotation.FromRoll( Time.Now * 320f );

		TravelledDistance += movement.Length;

		if ( TravelledDistance >= MaxDistance && Velocity.z >= 0f )
			Velocity = Velocity.WithZ( -Gravity * 0.25f );
	}

	bool HandleTouch( Vector3 previous, Vector3 next )
	{
		var hits = Scene.Trace
			.Sphere( HitRadius, previous, next )
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
				return TryHitPlayer( target, hit.HitPosition );

			return TryBurstOnFallingImpact( hit.HitPosition, null );
		}

		return false;
	}

	bool TryHitPlayer( PlayerCombat target, Vector3 hitPosition )
	{
		if ( !IsFalling() )
			return false;

		if ( target.IsValid() && !target.IsDead )
		{
			var amount = OwnerAttack.IsValid() ? OwnerAttack.ScaleMycellDamage( Damage ) : Damage;
			var impulse = Direction.Normal * RagdollImpulse + Vector3.Up * RagdollUpwardImpulse;
			target.ApplyDamage( new DamageEvent( Source, amount, DamageType.Magic, hitPosition, impulse ) );
		}

		Burst( hitPosition, target.GameObject );
		return true;
	}

	bool TryBurstOnFallingImpact( Vector3 hitPosition, GameObject attachTarget )
	{
		if ( !IsFalling() )
			return false;

		Burst( hitPosition, attachTarget );
		return true;
	}

	bool IsFalling()
	{
		return Velocity.z <= 0f;
	}

	void Burst( Vector3 position, GameObject attachTarget )
	{
		SpawnPoisonCloud( position, attachTarget );
		GameObject.Destroy();
	}

	void SpawnPoisonCloud( Vector3 position, GameObject attachTarget )
	{
		var parent = attachTarget.IsValid() ? attachTarget : null;
		var cloudObject = new GameObject( true, "Mycell Poison Cloud" );
		cloudObject.NetworkMode = NetworkMode.Never;
		cloudObject.WorldPosition = position;
		if ( parent.IsValid() )
			cloudObject.SetParent( parent, true );

		var cloud = cloudObject.Components.Create<MycellPoisonCloud>();
		cloud.Source = Source;
		cloud.OwnerAttack = OwnerAttack;
		cloud.Radius = CloudRadius;
		cloud.Duration = CloudDuration;
		cloud.TickInterval = CloudTickInterval;
		cloud.TickDamage = CloudTickDamage;
		cloud.MoveSpeedMultiplier = CloudMoveSpeedMultiplier;
		cloud.SlowDuration = CloudSlowDuration;
		cloud.OwnerMoveSpeedMultiplier = CloudOwnerMoveSpeedMultiplier;
		cloud.OwnerBuffDuration = CloudOwnerBuffDuration;
		cloud.FogPrefabPath = CloudFogPrefabPath;
		cloud.DebugColor = DebugColor;
	}

	static bool TryFindCombat( GameObject hitObject, out PlayerCombat combat )
	{
		combat = hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
		return combat.IsValid();
	}
}
