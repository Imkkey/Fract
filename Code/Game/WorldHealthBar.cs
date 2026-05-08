namespace Sandbox;

[Title( "World Health Bar" )]
[Category( "Battle Royale" )]
public sealed class WorldHealthBar : Component
{
	const int LayoutVersion = 3;

	[Property] public PlayerCombat Combat { get; set; }
	[Property] public string BackgroundTexturePath { get; set; } = "ui/pHpBg_padded.png";
	[Property] public string FillTexturePath { get; set; } = "ui/pHpFill_padded.png";
	[Property] public string FrameTexturePath { get; set; } = "ui/pHpFrame_padded.png";
	[Property] public Vector3 WorldOffset { get; set; } = new( 0f, -10f, 78f );
	[Property] public float RightOffset { get; set; } = 20f;
	[Property] public Vector2 PanelSize { get; set; } = new( 260f, 420f );
	[Property] public float RenderScale { get; set; } = 0.72f;
	[Property] public bool HideWhenFull { get; set; } = false;
	[Property] public bool HideWhenDead { get; set; } = true;
	[Property] public bool HideForOwner { get; set; } = true;

	GameObject RootObject { get; set; }
	WorldPanel WorldPanel { get; set; }
	WorldHealthBarPanel Panel { get; set; }
	int AppliedLayoutVersion { get; set; }

	string HealthBarObjectName => $"{GameObject.Name} Health Bar UI";
	string LegacySpriteObjectName => $"{GameObject.Name} Health Bar Sprites";
	string LegacyUiObjectName => $"{GameObject.Name} Health Bar";

	protected override void OnStart()
	{
		Combat ??= GetComponent<PlayerCombat>();
		CleanupLegacyBars();
		EnsureBarObjects();
	}

	protected override void OnUpdate()
	{
		if ( !Combat.IsValid() )
		{
			SetVisible( false );
			return;
		}

		var healthPercent = Combat.MaxHealth <= 0f ? 0f : (Combat.Health / Combat.MaxHealth).Clamp( 0f, 1f );
		var shouldShow = (!HideWhenDead || !Combat.IsDead)
			&& (!HideWhenFull || healthPercent < 0.999f)
			&& !ShouldHideForOwner();

		if ( !shouldShow )
		{
			SetVisible( false );
			return;
		}

		EnsureBarObjects();
		if ( !RootObject.IsValid() )
			return;

		SetVisible( true );
		RootObject.WorldPosition = GetBarPosition();
		UpdatePanel();
	}

	protected override void OnDestroy()
	{
		if ( RootObject.IsValid() )
			RootObject.Destroy();
	}

	void EnsureBarObjects()
	{
		if ( RootObject.IsValid() && AppliedLayoutVersion == LayoutVersion )
			return;

		if ( RootObject.IsValid() )
			RootObject.Destroy();

		RootObject = new GameObject( GameObject, false, HealthBarObjectName );
		RootObject.NetworkMode = NetworkMode.Never;
		RootObject.Enabled = true;
		AppliedLayoutVersion = LayoutVersion;

		WorldPanel = RootObject.Components.Create<WorldPanel>();
		Panel = RootObject.Components.Create<WorldHealthBarPanel>();
		UpdatePanel();
	}

	void UpdatePanel()
	{
		var backgroundTexturePath = NormalizeHealthTexturePath( BackgroundTexturePath );
		var fillTexturePath = NormalizeHealthTexturePath( FillTexturePath );
		var frameTexturePath = NormalizeHealthTexturePath( FrameTexturePath );

		if ( WorldPanel.IsValid() )
		{
			WorldPanel.PanelSize = PanelSize;
			WorldPanel.RenderScale = RenderScale;
			WorldPanel.LookAtCamera = true;
		}

		if ( !Panel.IsValid() )
			return;

		var rootPanel = Panel.Panel?.FindRootPanel();
		if ( rootPanel is not null )
		{
			rootPanel.PanelBounds = new Rect( 0f, 0f, PanelSize.x, PanelSize.y );
		}

		Panel.Combat = Combat;
		Panel.BackgroundTexturePath = backgroundTexturePath;
		Panel.FillTexturePath = fillTexturePath;
		Panel.FrameTexturePath = frameTexturePath;
	}

	string NormalizeHealthTexturePath( string texturePath )
	{
		return texturePath switch
		{
			"ui/pHpBg.png" => "ui/pHpBg_padded.png",
			"ui/pHpFill.png" => "ui/pHpFill_padded.png",
			"ui/pHpFrame.png" => "ui/pHpFrame_padded.png",
			_ => texturePath
		};
	}

	void CleanupLegacyBars()
	{
		DestroyBarsByName( LegacySpriteObjectName );
		DestroyBarsByName( LegacyUiObjectName );
		DestroyBarsByName( HealthBarObjectName );
	}

	void DestroyBarsByName( string name )
	{
		foreach ( var healthBarObject in Scene.Directory.FindByName( name ).ToArray() )
		{
			if ( !healthBarObject.IsValid() || healthBarObject == RootObject )
				continue;

			healthBarObject.Destroy();
		}
	}

	Vector3 GetBarPosition()
	{
		return WorldPosition + WorldOffset + GetCameraRight() * RightOffset;
	}

	bool ShouldHideForOwner()
	{
		if ( !HideForOwner || !GameObject.Network.IsOwner )
			return false;

		return GetComponent<PlayerController>().IsValid();
	}

	Vector3 GetCameraRight()
	{
		var camera = GetMainCamera();
		if ( camera.IsValid() )
			return camera.WorldRotation.Right.Normal;

		return Vector3.Right;
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

	void SetVisible( bool visible )
	{
		if ( RootObject.IsValid() )
			RootObject.Enabled = visible;
	}
}
