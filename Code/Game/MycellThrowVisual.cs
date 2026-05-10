namespace Sandbox;

[Title( "Mycell Throw Visual" )]
[Category( "Battle Royale" )]
public sealed class MycellThrowVisual : Component
{
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public Model MushroomModel { get; set; }
	[Property] public string WeaponBoneName { get; set; } = "hold_R";
	[Property] public string FallbackHandBoneName { get; set; } = "hand_R";
	[Property] public Vector3 MushroomLocalOffset { get; set; } = new( 2f, -1f, 0f );
	[Property] public Angles MushroomLocalAngles { get; set; } = new( 0f, 0f, 0f );
	[Property] public Vector3 MushroomLocalScale { get; set; } = new( 0.55f, 0.55f, 0.55f );
	[Property] public float MushroomAppearSeconds { get; set; } = 0.18f;
	[Property] public Citizen.CitizenAnimationHelper.HoldTypes IdleHoldType { get; set; } = Citizen.CitizenAnimationHelper.HoldTypes.None;
	[Property] public Citizen.CitizenAnimationHelper.Hand IdleHandedness { get; set; } = Citizen.CitizenAnimationHelper.Hand.Right;
	[Property] public Citizen.CitizenAnimationHelper.HoldTypes ThrowHoldType { get; set; } = Citizen.CitizenAnimationHelper.HoldTypes.Punch;
	[Property] public Citizen.CitizenAnimationHelper.Hand ThrowHandedness { get; set; } = Citizen.CitizenAnimationHelper.Hand.Right;
	[Property] public Citizen.CitizenAnimationHelper.HoldTypes UseHoldType { get; set; } = Citizen.CitizenAnimationHelper.HoldTypes.HoldItem;
	[Property] public Citizen.CitizenAnimationHelper.Hand UseHandedness { get; set; } = Citizen.CitizenAnimationHelper.Hand.Both;
	[Property] public float ThrowPoseSeconds { get; set; } = 0.24f;
	[Property] public float UsePoseSeconds { get; set; } = 0.42f;
	[Property] public float HideMushroomSeconds { get; set; } = 0.87f;
	[Property] public float PunchAttackValue { get; set; } = 1f;
	[Property] public float UseAttackValue { get; set; } = 1f;

	PlayerCharacter Character { get; set; }
	PlayerCombat Combat { get; set; }
	GameObject MushroomObject { get; set; }
	GameObject AttachedBoneObject { get; set; }
	TimeUntil ThrowPoseTime { get; set; }
	TimeUntil UsePoseTime { get; set; }
	TimeUntil ShowMushroomTime { get; set; }
	TimeSince TimeSinceMushroomShown { get; set; } = 999f;
	bool WasMushroomVisible { get; set; }

	protected override void OnStart()
	{
		Character = GetComponent<PlayerCharacter>();
		Combat = GetComponent<PlayerCombat>();
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		MushroomModel ??= Model.Load( "models/fungus2.vmdl" );
		EnsureMushroomObject();
		SetMushroomVisible( false );
	}

	protected override void OnUpdate()
	{
		if ( !ShouldUseMycellPose() )
		{
			SetMushroomVisible( false );
			return;
		}

		ApplyCurrentHoldAnimation();

		if ( AttachMushroomToHandBone() )
		{
			ApplyMushroomLocalTransform();
			SetMushroomVisible( ShouldShowMushroom() );
		}
	}

	protected override void OnPreRender()
	{
		if ( ShouldUseMycellPose() )
		{
			ApplyCurrentHoldAnimation();
			ApplyMushroomLocalTransform();
		}
	}

	public void PlayThrowVisual()
	{
		PlayThrowVisual( HideMushroomSeconds + MushroomAppearSeconds );
	}

	public void PlayThrowVisual( float cooldown )
	{
		if ( !BodyRenderer.IsValid() )
			return;

		ThrowPoseTime = ThrowPoseSeconds;
		ShowMushroomTime = GetHideSecondsBeforeAppear( cooldown );
		SetMushroomVisible( false );
		ApplyHoldAnimation( ThrowHoldType, ThrowHandedness );
		BodyRenderer.Set( "holdtype_attack", PunchAttackValue );
		BodyRenderer.Set( "b_attack", true );
	}

	public void PlayUseVisual()
	{
		if ( !BodyRenderer.IsValid() )
			return;

		UsePoseTime = UsePoseSeconds;
		ApplyHoldAnimation( UseHoldType, UseHandedness );
		BodyRenderer.Set( "holdtype_attack", UseAttackValue );
		BodyRenderer.Set( "b_attack", true );
	}

	float GetHideSecondsBeforeAppear( float cooldown )
	{
		if ( cooldown <= 0f )
			return 0f;

		return (cooldown - MushroomAppearSeconds).Clamp( 0f, cooldown );
	}

	void EnsureMushroomObject()
	{
		if ( MushroomObject.IsValid() || !MushroomModel.IsValid() )
			return;

		MushroomObject = new GameObject( false, "Mycell Hand Mushroom" );
		MushroomObject.NetworkMode = NetworkMode.Never;

		var renderer = MushroomObject.Components.Create<ModelRenderer>();
		renderer.Model = MushroomModel;
	}

	bool AttachMushroomToHandBone()
	{
		EnsureMushroomObject();

		if ( !BodyRenderer.IsValid() || !MushroomObject.IsValid() )
			return false;

		var boneObject = GetWeaponBoneObject();
		if ( !boneObject.IsValid() )
			return false;

		if ( AttachedBoneObject == boneObject )
			return true;

		MushroomObject.SetParent( boneObject, false );
		AttachedBoneObject = boneObject;
		ApplyMushroomLocalTransform();
		return true;
	}

	GameObject GetWeaponBoneObject()
	{
		if ( !string.IsNullOrWhiteSpace( WeaponBoneName ) )
		{
			var weaponBone = BodyRenderer.GetBoneObject( WeaponBoneName );
			if ( weaponBone.IsValid() )
				return weaponBone;
		}

		if ( !string.IsNullOrWhiteSpace( FallbackHandBoneName ) )
		{
			var fallbackBone = BodyRenderer.GetBoneObject( FallbackHandBoneName );
			if ( fallbackBone.IsValid() )
				return fallbackBone;
		}

		return null;
	}

	void ApplyMushroomLocalTransform()
	{
		if ( !MushroomObject.IsValid() )
			return;

		MushroomObject.LocalPosition = MushroomLocalOffset;
		MushroomObject.LocalRotation = MushroomLocalAngles.ToRotation();
		MushroomObject.LocalScale = MushroomLocalScale * GetMushroomAppearScale();
	}

	void SetMushroomVisible( bool visible )
	{
		if ( visible && !WasMushroomVisible )
			TimeSinceMushroomShown = 0f;

		WasMushroomVisible = visible;

		if ( MushroomObject.IsValid() )
			MushroomObject.Enabled = visible;
	}

	bool ShouldShowMushroom()
	{
		return ShowMushroomTime <= 0f;
	}

	float GetMushroomAppearScale()
	{
		if ( MushroomAppearSeconds <= 0f )
			return 1f;

		var progress = (TimeSinceMushroomShown / MushroomAppearSeconds).Clamp( 0f, 1f );
		return progress * progress * (3f - 2f * progress);
	}

	void ApplyHoldAnimation( Citizen.CitizenAnimationHelper.HoldTypes holdType, Citizen.CitizenAnimationHelper.Hand handedness )
	{
		if ( !BodyRenderer.IsValid() )
			return;

		BodyRenderer.Set( "holdtype", (int)holdType );
		BodyRenderer.Set( "holdtype_handedness", (int)handedness );
	}

	void ApplyCurrentHoldAnimation()
	{
		if ( UsePoseTime > 0f )
		{
			ApplyHoldAnimation( UseHoldType, UseHandedness );
			return;
		}

		if ( ThrowPoseTime > 0f )
		{
			ApplyHoldAnimation( ThrowHoldType, ThrowHandedness );
			return;
		}

		ApplyHoldAnimation( IdleHoldType, IdleHandedness );
	}

	bool ShouldUseMycellPose()
	{
		return Character.IsValid()
			&& Character.CurrentCharacter == CharacterId.Mycell
			&& (!Combat.IsValid() || !Combat.IsDead);
	}

	protected override void OnDestroy()
	{
		if ( MushroomObject.IsValid() )
			MushroomObject.Destroy();
	}
}
