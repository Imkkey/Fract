using System.Collections.Generic;

namespace Sandbox;

[Title( "Mycell Mushroom Step Trap" )]
[Category( "Battle Royale" )]
public sealed class MycellMushroomStepTrap : Component
{
	public GameObject Source { get; set; }
	public MycellAttack OwnerAttack { get; set; }
	public float Lifetime { get; set; } = 10f;
	public float TriggerRadius { get; set; } = 80f;
	public float ExplosionDamage { get; set; } = 35f;
	public float SlowMultiplier { get; set; } = 0.75f;
	public float SlowDuration { get; set; } = 2f;
	public float WalkBounceVelocity { get; set; } = 520f;
	public float FallBounceVelocity { get; set; } = 700f;
	public float PreventGroundingTime { get; set; } = 0.18f;
	public float MinBounceHeightOffset { get; set; } = -8f;
	public float MaxBounceHeightOffset { get; set; } = 24f;
	public float BounceFootprintWidth { get; set; } = 34f;
	public float BounceFootprintDepth { get; set; } = 46f;
	public float BounceCooldown { get; set; } = 0.45f;
	public float VisualBounceDuration { get; set; } = 0.18f;
	public float VisualBounceStrength { get; set; } = 0.16f;
	public bool DestroyOnBounce { get; set; } = false;
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
	public Vector3 VisualScale { get; set; } = new( 0.7f, 0.7f, 0.7f );

	TimeUntil LifeTime { get; set; }
	GameObject VisualObject { get; set; }
	Dictionary<PlayerCombat, TimeUntil> BounceCooldowns { get; } = new();
	float VisualBounceTime { get; set; }

	protected override void OnStart()
	{
		LifeTime = Lifetime;
		SpawnVisual();
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f )
		{
			GameObject.Destroy();
			return;
		}

		if ( Networking.IsHost )
			TryTrigger();

		UpdateVisualBounce();
	}

	void TryTrigger()
	{
		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target.IsDead )
				continue;

			if ( !IsInsideMushroomFootprint( target.WorldPosition ) )
				continue;

			var heightOffset = target.WorldPosition.z - WorldPosition.z;
			if ( heightOffset < MinBounceHeightOffset || heightOffset > MaxBounceHeightOffset )
				continue;

			if ( IsTargetOnCooldown( target ) )
				continue;

			if ( target.GameObject == Source )
			{
				BounceTarget( target );
				BounceCooldowns[target] = BounceCooldown;
				if ( DestroyOnBounce )
				{
					SpawnMyceliumPatch();
					GameObject.Destroy();
				}
			}
			else
			{
				BurstOnEnemy( target );
				GameObject.Destroy();
			}

			return;
		}
	}

	bool IsInsideMushroomFootprint( Vector3 position )
	{
		var flatOffset = (position - WorldPosition).WithZ( 0f );
		var localX = flatOffset.Dot( WorldRotation.Right.WithZ( 0f ).Normal );
		var localY = flatOffset.Dot( WorldRotation.Forward.WithZ( 0f ).Normal );
		localX = localX < 0f ? -localX : localX;
		localY = localY < 0f ? -localY : localY;

		var halfWidth = BounceFootprintWidth * 0.5f;
		var halfDepth = BounceFootprintDepth * 0.5f;
		if ( localX > halfWidth || localY > halfDepth )
			return false;

		var ellipseX = localX / halfWidth;
		var ellipseY = localY / halfDepth;
		return ellipseX * ellipseX + ellipseY * ellipseY <= 1f;
	}

	void BounceTarget( PlayerCombat target )
	{
		var controller = target.GetComponent<PlayerController>();
		if ( !controller.IsValid() )
			return;

		var strongBounce = IsFallingOntoTrap( controller );
		controller.PreventGrounding( PreventGroundingTime );
		controller.Jump( Vector3.Up * (strongBounce ? FallBounceVelocity : WalkBounceVelocity) );
		VisualBounceTime = VisualBounceDuration;
	}

	void UpdateVisualBounce()
	{
		if ( !VisualObject.IsValid() )
			return;

		if ( VisualBounceTime <= 0f )
		{
			VisualObject.LocalScale = VisualScale;
			return;
		}

		VisualBounceTime -= Time.Delta;
		var progress = (1f - VisualBounceTime / VisualBounceDuration).Clamp( 0f, 1f );
		var pulse = progress < 0.5f ? progress * 2f : (1f - progress) * 2f;
		var sideScale = 1f + VisualBounceStrength * pulse;
		var heightScale = 1f - VisualBounceStrength * 0.45f * pulse;
		VisualObject.LocalScale = new Vector3( VisualScale.x * sideScale, VisualScale.y * sideScale, VisualScale.z * heightScale );
	}

	bool IsFallingOntoTrap( PlayerController controller )
	{
		return controller.IsAirborne && controller.Velocity.z <= 0f;
	}

	void BurstOnEnemy( PlayerCombat target )
	{
		var amount = OwnerAttack.IsValid() ? OwnerAttack.ScaleMycellDamage( ExplosionDamage ) : ExplosionDamage;
		target.ApplyDamage( new DamageEvent( Source, amount, DamageType.Magic, target.WorldPosition + Vector3.Up * 36f ) );
		target.ApplyMoveSlow( SlowMultiplier, SlowDuration );
		SpawnPoisonCloud( WorldPosition );
		SpawnMyceliumPatch();
	}

	void SpawnPoisonCloud( Vector3 position )
	{
		var cloudObject = new GameObject( true, "Mycell Trap Poison Cloud" );
		cloudObject.NetworkMode = NetworkMode.Never;
		cloudObject.WorldPosition = position;

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
		cloud.DebugColor = MushroomTint;
	}

	bool IsTargetOnCooldown( PlayerCombat target )
	{
		if ( !BounceCooldowns.TryGetValue( target, out var cooldown ) )
			return false;

		if ( cooldown <= 0f )
		{
			BounceCooldowns.Remove( target );
			return false;
		}

		return true;
	}

	void SpawnMyceliumPatch()
	{
		var patchObject = new GameObject( true, "Mycell Trap Mycelium Patch" );
		patchObject.NetworkMode = NetworkMode.Never;
		patchObject.WorldPosition = WorldPosition;

		var patch = patchObject.Components.Create<MycellMyceliumPatch>();
		patch.Source = Source;
		patch.Radius = MyceliumRadius;
		patch.Duration = MyceliumDuration;
		patch.DebugColor = MushroomTint;
	}

	void SpawnVisual()
	{
		if ( !MushroomModel.IsValid() )
			MushroomModel = Model.Load( "models/fungus2.vmdl" );

		if ( !MushroomModel.IsValid() )
			return;

		VisualObject = new GameObject( false, "Mushroom Step Trap Visual" );
		VisualObject.NetworkMode = NetworkMode.Never;
		VisualObject.Enabled = true;
		VisualObject.SetParent( GameObject, false );
		VisualObject.LocalPosition = Vector3.Zero;
		VisualObject.LocalScale = VisualScale;

		var renderer = VisualObject.Components.Create<ModelRenderer>();
		renderer.Model = MushroomModel;
		renderer.Tint = MushroomTint;
	}

	protected override void OnDestroy()
	{
		if ( VisualObject.IsValid() )
			VisualObject.Destroy();
	}
}
