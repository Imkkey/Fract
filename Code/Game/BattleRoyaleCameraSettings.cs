namespace Sandbox;

[Title( "Battle Royale Camera Settings" )]
[Category( "Battle Royale" )]
public sealed class BattleRoyaleCameraSettings : Component
{
	[Property] public CameraComponent TargetCamera { get; set; }
	[Property] public float FieldOfView { get; set; } = 80f;
	[Property] public float MinFieldOfView { get; set; } = 45f;
	[Property] public float MaxFieldOfView { get; set; } = 120f;
	[Property, Group( "Valtryek Storm" )] public float StormFieldOfView { get; set; } = 100f;
	[Property, Group( "Valtryek Storm" )] public float StormFovLerpSpeed { get; set; } = 7f;
	[Property, Group( "Valtryek Storm" )] public Color StormVignetteColor { get; set; } = new( 0.03f, 0.22f, 1f, 1f );
	[Property, Group( "Valtryek Storm" )] public float StormVignetteIntensity { get; set; } = 0.42f;
	[Property, Group( "Valtryek Storm" )] public float StormVignetteRoundness { get; set; } = 0.85f;
	[Property, Group( "Valtryek Storm" )] public float StormVignetteSmoothness { get; set; } = 0.62f;

	Vignette StormVignette { get; set; }

	protected override void OnUpdate()
	{
		if ( !GameObject.Network.IsOwner )
			return;

		var camera = GetTargetCamera();
		if ( !camera.IsValid() )
			return;

		var stormActive = IsValtryekStormActive();
		var targetFov = stormActive
			? StormFieldOfView
			: FieldOfView;

		var lerpAmount = (Time.Delta * StormFovLerpSpeed).Clamp( 0f, 1f );
		camera.FieldOfView = camera.FieldOfView.LerpTo( targetFov.Clamp( MinFieldOfView, MaxFieldOfView ), lerpAmount );
		UpdateStormVignette( camera, stormActive );
	}

	void UpdateStormVignette( CameraComponent camera, bool stormActive )
	{
		StormVignette = GetOrCreateStormVignette( camera );
		if ( !StormVignette.IsValid() )
			return;

		var targetIntensity = stormActive ? StormVignetteIntensity : 0f;
		StormVignette.Intensity = StormVignette.Intensity.LerpTo( targetIntensity.Clamp( 0f, 1f ), (Time.Delta * StormFovLerpSpeed).Clamp( 0f, 1f ) );
		StormVignette.Color = StormVignetteColor;
		StormVignette.Center = new Vector2( 0.5f, 0.5f );
		StormVignette.Roundness = StormVignetteRoundness;
		StormVignette.Smoothness = StormVignetteSmoothness;
		StormVignette.Enabled = StormVignette.Intensity > 0.01f || stormActive;
	}

	Vignette GetOrCreateStormVignette( CameraComponent camera )
	{
		if ( StormVignette.IsValid() )
			return StormVignette;

		var vignette = camera.GameObject.Components.Get<Vignette>( FindMode.EverythingInSelf );
		if ( vignette.IsValid() )
			return vignette;

		return camera.GameObject.Components.Create<Vignette>();
	}

	bool IsValtryekStormActive()
	{
		var character = GetComponent<PlayerCharacter>();
		if ( !character.IsValid() || character.CurrentCharacter != CharacterId.Valtryek )
			return false;

		var attack = GetComponent<ValtryekLightningAttack>();
		return attack.IsValid() && attack.IsStormState;
	}

	CameraComponent GetTargetCamera()
	{
		if ( TargetCamera.IsValid() )
			return TargetCamera;

		foreach ( var camera in Scene.GetAllComponents<CameraComponent>() )
		{
			if ( camera.IsValid() && camera.IsMainCamera )
			{
				TargetCamera = camera;
				return camera;
			}
		}

		return null;
	}
}
