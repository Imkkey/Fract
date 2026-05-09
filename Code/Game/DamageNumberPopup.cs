namespace Sandbox;

[Title( "Damage Number Popup" )]
[Category( "Battle Royale" )]
public sealed class DamageNumberPopup : Component
{
	[Property] public float Lifetime { get; set; } = 0.85f;
	[Property] public float RiseSpeed { get; set; } = 42f;
	[Property] public Vector2 PanelSize { get; set; } = new( 120f, 64f );
	[Property] public float RenderScale { get; set; } = 0.48f;
	[Property] public float ScaleReferenceDistance { get; set; } = 600f;
	[Property] public float MinRenderScaleMultiplier { get; set; } = 0.9f;
	[Property] public float MaxRenderScaleMultiplier { get; set; } = 3.2f;

	public float Amount { get; set; }
	public DamageType DamageType { get; set; }

	WorldPanel WorldPanel { get; set; }
	DamageNumberPanel Panel { get; set; }
	float Age { get; set; }
	Vector3 Drift { get; set; }

	protected override void OnStart()
	{
		GameObject.NetworkMode = NetworkMode.Never;

		Drift = new Vector3( Game.Random.Next( -14, 15 ), Game.Random.Next( -14, 15 ), RiseSpeed );

		WorldPanel = Components.Create<WorldPanel>();
		WorldPanel.PanelSize = PanelSize;
		WorldPanel.LookAtCamera = true;

		Panel = Components.Create<DamageNumberPanel>();
		UpdatePanel();
	}

	protected override void OnUpdate()
	{
		Age += Time.Delta;

		if ( Age >= Lifetime )
		{
			GameObject.Destroy();
			return;
		}

		WorldPosition += Drift * Time.Delta;
		UpdatePanel();
	}

	void UpdatePanel()
	{
		if ( WorldPanel.IsValid() )
			WorldPanel.RenderScale = GetDistanceAdjustedRenderScale();

		if ( !Panel.IsValid() )
			return;

		var progress = (Age / Lifetime).Clamp( 0f, 1f );
		var bounceDelta = progress * 2f - 1f;
		if ( bounceDelta < 0f )
			bounceDelta = -bounceDelta;

		var bounce = 1f - bounceDelta;

		Panel.Amount = Amount;
		Panel.DamageType = DamageType;
		Panel.Scale = 0.86f + bounce * 0.38f;
		Panel.Opacity = 1f - progress * progress;
	}

	float GetDistanceAdjustedRenderScale()
	{
		var camera = GetMainCamera();
		if ( !camera.IsValid() || ScaleReferenceDistance <= 1f )
			return RenderScale;

		var multiplier = ((camera.WorldPosition - WorldPosition).Length / ScaleReferenceDistance)
			.Clamp( MinRenderScaleMultiplier, MaxRenderScaleMultiplier );

		return RenderScale * multiplier;
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
}
