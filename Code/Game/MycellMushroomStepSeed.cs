namespace Sandbox;

[Title( "Mycell Mushroom Step Seed" )]
[Category( "Battle Royale" )]
public sealed class MycellMushroomStepSeed : Component
{
	public GameObject Source { get; set; }
	public MycellAttack OwnerAttack { get; set; }
	public Vector3 Direction { get; set; }
	public float Speed { get; set; } = 780f;
	public float UpwardVelocity { get; set; } = 180f;
	public float Gravity { get; set; } = 780f;
	public float MaxLifetime { get; set; } = 3f;
	public float HitRadius { get; set; } = 18f;
	public float TrapLifetime { get; set; } = 10f;
	public float TrapRadius { get; set; } = 80f;
	public float TrapDamage { get; set; } = 35f;
	public float TrapSlowMultiplier { get; set; } = 0.75f;
	public float TrapSlowDuration { get; set; } = 2f;
	public float CloudRadius { get; set; } = 86f;
	public float CloudDuration { get; set; } = 3.2f;
	public float CloudTickInterval { get; set; } = 0.5f;
	public float CloudTickDamage { get; set; } = 12f;
	public float CloudMoveSpeedMultiplier { get; set; } = 0.92f;
	public float CloudSlowDuration { get; set; } = 0.65f;
	public float CloudOwnerMoveSpeedMultiplier { get; set; } = 1.08f;
	public float CloudOwnerBuffDuration { get; set; } = 0.65f;
	public string CloudFogPrefabPath { get; set; } = "prefabs/shroomfog.prefab";
	public float MyceliumDuration { get; set; } = 6f;
	public float MyceliumRadius { get; set; } = 105f;
	public Model MushroomModel { get; set; }
	public Color MushroomTint { get; set; } = new( 0.54f, 1f, 0.22f, 1f );

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
		var next = previous + Velocity * Time.Delta;

		if ( HandleTouch( previous, next ) )
			return;

		WorldPosition = next;
		if ( Velocity.Length > 0.01f )
			WorldRotation = Rotation.LookAt( Velocity.Normal, Vector3.Up ) * Rotation.FromRoll( Time.Now * 280f );
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

			if ( Velocity.z > 0f )
				continue;

			Land( hit.HitPosition );
			return true;
		}

		return false;
	}

	void Land( Vector3 position )
	{
		SpawnTrap( position );
		SpawnMyceliumPatch( position );
		GameObject.Destroy();
	}

	void SpawnTrap( Vector3 position )
	{
		var trapObject = new GameObject( true, "Mycell Mushroom Step Trap" );
		trapObject.NetworkMode = NetworkMode.Never;
		trapObject.WorldPosition = position;

		var trap = trapObject.Components.Create<MycellMushroomStepTrap>();
		trap.Source = Source;
		trap.OwnerAttack = OwnerAttack;
		trap.Lifetime = TrapLifetime;
		trap.TriggerRadius = TrapRadius;
		trap.ExplosionDamage = TrapDamage;
		trap.SlowMultiplier = TrapSlowMultiplier;
		trap.SlowDuration = TrapSlowDuration;
		trap.CloudRadius = CloudRadius;
		trap.CloudDuration = CloudDuration;
		trap.CloudTickInterval = CloudTickInterval;
		trap.CloudTickDamage = CloudTickDamage;
		trap.CloudMoveSpeedMultiplier = CloudMoveSpeedMultiplier;
		trap.CloudSlowDuration = CloudSlowDuration;
		trap.CloudOwnerMoveSpeedMultiplier = CloudOwnerMoveSpeedMultiplier;
		trap.CloudOwnerBuffDuration = CloudOwnerBuffDuration;
		trap.CloudFogPrefabPath = CloudFogPrefabPath;
		trap.BounceFootprintWidth = TrapRadius * 1.35f;
		trap.BounceFootprintDepth = TrapRadius;
		trap.MyceliumDuration = MyceliumDuration;
		trap.MyceliumRadius = MyceliumRadius;
		trap.MushroomModel = MushroomModel;
		trap.MushroomTint = MushroomTint;
		trap.VisualScale = new Vector3( 7f, 7f, 3f );
	}

	void SpawnMyceliumPatch( Vector3 position )
	{
		var patchObject = new GameObject( true, "Mycell Step Mycelium Patch" );
		patchObject.NetworkMode = NetworkMode.Never;
		patchObject.WorldPosition = position;

		var patch = patchObject.Components.Create<MycellMyceliumPatch>();
		patch.Source = Source;
		patch.Radius = MyceliumRadius;
		patch.Duration = MyceliumDuration;
		patch.DebugColor = MushroomTint;
	}
}
