namespace Sandbox;

[Title( "World Health Bar" )]
[Category( "Battle Royale" )]
public sealed class WorldHealthBar : Component
{
	const int LayoutVersion = 4;

	[Property] public PlayerCombat Combat { get; set; }
	[Property] public string BackgroundTexturePath { get; set; } = "ui/pHpBg_padded.png";
	[Property] public string FillTexturePath { get; set; } = "ui/pHpFill_padded.png";
	[Property] public string FrameTexturePath { get; set; } = "ui/pHpFrame_padded.png";
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public string HeadBoneName { get; set; } = "head";
	[Property] public Vector3 HeadLocalOffset { get; set; } = new( 0f, 0f, 28f );
	[Property] public Vector3 WorldOffset { get; set; } = new( 0f, -10f, 78f );
	[Property] public float FrontRightOffset { get; set; } = 0f;
	[Property] public float BackRightOffset { get; set; } = 35f;
	[Property] public float FrontViewerDepthOffset { get; set; } = 0f;
	[Property] public float BackViewerDepthOffset { get; set; } = 50f;
	[Property] public Vector2 PanelSize { get; set; } = new( 260f, 420f );
	[Property] public float RenderScale { get; set; } = 0.72f;
	[Property] public float ScaleReferenceDistance { get; set; } = 420f;
	[Property] public float MinRenderScaleMultiplier { get; set; } = 0.8f;
	[Property] public float MaxRenderScaleMultiplier { get; set; } = 1.25f;
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
		BodyRenderer ??= Combat.IsValid()
			? Combat.BodyRenderer
			: GameObject.Components.Get<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );
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
			WorldPanel.RenderScale = GetDistanceAdjustedRenderScale();
			WorldPanel.LookAtCamera = true;
		}

		if ( !Panel.IsValid() )
			return;

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
		var anchorPosition = GetAnchorPosition();
		var viewDirection = GetViewerDirection( anchorPosition );
		var frontAmount = GetFrontViewAmount( viewDirection );
		return anchorPosition
			+ GetViewerRelativeRight( viewDirection ) * GetRightOffset( frontAmount )
			+ viewDirection * GetViewerDepthOffset( frontAmount );
	}

	Vector3 GetAnchorPosition()
	{
		var headBone = GetHeadBoneObject();
		if ( headBone.IsValid() )
			return headBone.WorldPosition + headBone.WorldRotation * HeadLocalOffset;

		return WorldPosition + WorldOffset;
	}

	GameObject GetHeadBoneObject()
	{
		if ( !BodyRenderer.IsValid() )
		{
			BodyRenderer = Combat.IsValid()
				? Combat.BodyRenderer
				: GameObject.Components.Get<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );
		}

		if ( !BodyRenderer.IsValid() || string.IsNullOrWhiteSpace( HeadBoneName ) )
			return null;

		return BodyRenderer.GetBoneObject( HeadBoneName );
	}

	bool ShouldHideForOwner()
	{
		if ( !HideForOwner || !GameObject.Network.IsOwner )
			return false;

		return GetComponent<PlayerController>().IsValid();
	}

	Vector3 GetViewerDirection( Vector3 anchorPosition )
	{
		var camera = GetMainCamera();
		if ( camera.IsValid() )
		{
			var toCamera = (camera.WorldPosition - anchorPosition).WithZ( 0f );
			if ( toCamera.Length > 0.01f )
				return toCamera.Normal;
		}

		return Vector3.Forward;
	}

	Vector3 GetViewerRelativeRight( Vector3 viewDirection )
	{
		return new Vector3( -viewDirection.y, viewDirection.x, 0f ).Normal;
	}

	float GetDistanceAdjustedRenderScale()
	{
		var camera = GetMainCamera();
		if ( !camera.IsValid() || !RootObject.IsValid() || ScaleReferenceDistance <= 1f )
			return RenderScale;

		var distance = (camera.WorldPosition - RootObject.WorldPosition).Length;
		var multiplier = (distance / ScaleReferenceDistance).Clamp( MinRenderScaleMultiplier, MaxRenderScaleMultiplier );
		return RenderScale * multiplier;
	}

	float GetRightOffset( float frontAmount )
	{
		return BackRightOffset + (FrontRightOffset - BackRightOffset) * frontAmount;
	}

	float GetViewerDepthOffset( float frontAmount )
	{
		return BackViewerDepthOffset + (FrontViewerDepthOffset - BackViewerDepthOffset) * frontAmount;
	}

	float GetFrontViewAmount( Vector3 viewDirection )
	{
		var forward = WorldRotation.Forward.WithZ( 0f );
		if ( forward.Length <= 0.01f )
			return 1f;

		return ((forward.Normal.Dot( viewDirection ) + 1f) * 0.5f).Clamp( 0f, 1f );
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
