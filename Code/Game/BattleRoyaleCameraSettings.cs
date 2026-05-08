namespace Sandbox;

[Title( "Battle Royale Camera Settings" )]
[Category( "Battle Royale" )]
public sealed class BattleRoyaleCameraSettings : Component
{
	[Property] public CameraComponent TargetCamera { get; set; }
	[Property] public float FieldOfView { get; set; } = 80f;
	[Property] public float MinFieldOfView { get; set; } = 45f;
	[Property] public float MaxFieldOfView { get; set; } = 120f;

	protected override void OnUpdate()
	{
		if ( !GameObject.Network.IsOwner )
			return;

		var camera = GetTargetCamera();
		if ( !camera.IsValid() )
			return;

		camera.FieldOfView = FieldOfView.Clamp( MinFieldOfView, MaxFieldOfView );
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
