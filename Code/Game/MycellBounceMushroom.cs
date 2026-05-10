namespace Sandbox;

[Title( "Mycell Bounce Mushroom" )]
[Category( "Battle Royale" )]
public sealed class MycellBounceMushroom : Component
{
	public GameObject Source { get; set; }
	public bool BounceOnlySource { get; set; } = true;
	public float Lifetime { get; set; } = 10f;
	public float ArmDelay { get; set; } = 0.35f;
	public float TriggerRadius { get; set; } = 74f;
	public float UpDistance { get; set; } = 180f;
	public Model MushroomModel { get; set; }
	public Color MushroomTint { get; set; } = new( 0.54f, 1f, 0.22f, 1f );

	TimeUntil LifeTime { get; set; }
	TimeUntil ArmTime { get; set; }
	GameObject VisualObject { get; set; }

	protected override void OnStart()
	{
		LifeTime = Lifetime;
		ArmTime = ArmDelay;
		SpawnVisual();
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f )
		{
			GameObject.Destroy();
			return;
		}

		if ( Networking.IsHost && ArmTime <= 0f )
			TryBouncePlayers();
	}

	void TryBouncePlayers()
	{
		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target.IsDead )
				continue;

			if ( BounceOnlySource && target.GameObject != Source )
				continue;

			var toTarget = target.WorldPosition - WorldPosition;
			if ( toTarget.WithZ( 0f ).Length > TriggerRadius )
				continue;

			BounceTarget( target );
		}
	}

	void BounceTarget( PlayerCombat target )
	{
		var targetPosition = target.WorldPosition + Vector3.Up * UpDistance;
		target.WorldPosition = targetPosition;
		GameObject.Destroy();
	}

	void SpawnVisual()
	{
		if ( !MushroomModel.IsValid() )
			MushroomModel = Model.Load( "models/fungus2.vmdl" );

		if ( !MushroomModel.IsValid() )
			return;

		VisualObject = new GameObject( false, "Mycell Bounce Mushroom Visual" );
		VisualObject.NetworkMode = NetworkMode.Never;
		VisualObject.SetParent( GameObject, false );
		VisualObject.LocalPosition = Vector3.Zero;
		VisualObject.LocalScale = new Vector3( 0.78f, 0.78f, 0.78f );

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
