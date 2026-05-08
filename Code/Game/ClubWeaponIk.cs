using System;

namespace Sandbox;

[Title( "Club Weapon IK" )]
[Category( "Battle Royale" )]
public sealed class ClubWeaponIk : Component
{
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public GameObject ClubObject { get; set; }
	[Property] public Model ClubModel { get; set; }
	[Property] public string WeaponBoneName { get; set; } = "hold_R";
	[Property] public string FallbackHandBoneName { get; set; } = "hand_R";
	[Property] public Vector3 ClubLocalOffset { get; set; } = Vector3.Zero;
	[Property] public Angles ClubLocalAngles { get; set; } = new( 0f, 0f, 0f );
	[Property] public Vector3 LeftGripLocalOffset { get; set; } = new( 0f, 0f, 22f );
	[Property] public float SwingAttackValue { get; set; } = 3f;
	[Property, Group( "Visual Swing" )] public float SwingVisualDuration { get; set; } = 0.7f;
	[Property, Group( "Visual Swing" )] public Vector3 SwingVisualOffset { get; set; } = new( 8f, -5f, 2f );
	[Property, Group( "Visual Swing" )] public Angles SwingVisualStartAngles { get; set; } = new( -25f, -35f, 18f );
	[Property, Group( "Visual Swing" )] public Angles SwingVisualEndAngles { get; set; } = new( 22f, 42f, -12f );

	PlayerCharacter Character { get; set; }
	PlayerCombat Combat { get; set; }
	GameObject AttachedBoneObject { get; set; }
	bool WasActive { get; set; }
	bool OwnsClubObject { get; set; }
	TimeSince TimeSinceSwingVisual { get; set; } = 999f;

	protected override void OnStart()
	{
		Character = GetComponent<PlayerCharacter>();
		Combat = GetComponent<PlayerCombat>();

		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		UsePrefabClubAsModelSource();
		ClubModel ??= Model.Load( "models/dubina.vmdl" );
		EnsureClubObject();
		SetVisible( false );
	}

	protected override void OnUpdate()
	{
		var shouldShow = Character.IsValid()
			&& Character.CurrentCharacter == CharacterId.ClubBrawler
			&& (!Combat.IsValid() || !Combat.IsDead);

		if ( !shouldShow )
		{
			SetVisible( false );
			if ( WasActive )
			{
				ClearHoldAnimation();
				ClearIk();
				WasActive = false;
			}

			return;
		}

		EnsureClubObject();

		if ( !BodyRenderer.IsValid() || !ClubObject.IsValid() )
			return;

		WasActive = true;
		ApplyHoldAnimation();

		if ( !AttachClubToHandBone() )
		{
			SetVisible( false );
			ClearIk();
			return;
		}

		ApplyClubLocalTransform();
		SetVisible( true );
		ApplyLeftHandIk();
	}

	protected override void OnPreRender()
	{
		if ( !ClubObject.IsValid() || !BodyRenderer.IsValid() || !AttachedBoneObject.IsValid() || !ClubObject.Enabled )
			return;

		ApplyHoldAnimation();
		ApplyClubLocalTransform();
		ApplyLeftHandIk();
	}

	bool AttachClubToHandBone()
	{
		var boneObject = GetWeaponBoneObject();
		if ( !boneObject.IsValid() )
			return false;

		if ( AttachedBoneObject == boneObject )
			return true;

		ClubObject.SetParent( boneObject, false );
		AttachedBoneObject = boneObject;
		ApplyClubLocalTransform();
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

	void EnsureClubObject()
	{
		if ( ClubObject.IsValid() )
		{
			return;
		}

		if ( !ClubModel.IsValid() )
			return;

		ClubObject = new GameObject( false, "Club Hand Visual" );
		ClubObject.NetworkMode = NetworkMode.Never;
		OwnsClubObject = true;

		var renderer = ClubObject.Components.Create<ModelRenderer>();
		renderer.Model = ClubModel;
	}

	void UsePrefabClubAsModelSource()
	{
		if ( !ClubObject.IsValid() )
			return;

		var renderer = ClubObject.Components.Get<ModelRenderer>();
		if ( renderer.IsValid() )
		{
			ClubModel ??= renderer.Model;
		}

		ClubObject.Enabled = false;
		ClubObject = null;
	}

	void ApplyClubLocalTransform()
	{
		if ( !ClubObject.IsValid() )
			return;

		var swingAmount = GetSwingVisualAmount();
		ClubObject.LocalPosition = ClubLocalOffset + SwingVisualOffset * swingAmount;
		ClubObject.LocalRotation = ClubLocalAngles.ToRotation() * GetSwingVisualRotation();
	}

	void ApplyLeftHandIk()
	{
		var gripRotation = ClubObject.WorldRotation;
		var leftHand = ClubObject.WorldPosition + gripRotation * LeftGripLocalOffset;

		BodyRenderer.ClearIk( "hand_right" );
		BodyRenderer.SetIk( "hand_left", CreateGripTransform( leftHand, gripRotation ) );
	}

	public void PlaySwingVisual()
	{
		if ( !BodyRenderer.IsValid() )
			return;

		ApplyHoldAnimation();
		BodyRenderer.Set( "holdtype_attack", SwingAttackValue );
		BodyRenderer.Set( "b_attack", true );
		TimeSinceSwingVisual = 0f;
	}

	void ApplyHoldAnimation()
	{
		BodyRenderer.Set( "holdtype", (int)Citizen.CitizenAnimationHelper.HoldTypes.Swing );
		BodyRenderer.Set( "holdtype_handedness", (int)Citizen.CitizenAnimationHelper.Hand.Both );
	}

	void ClearHoldAnimation()
	{
		if ( !BodyRenderer.IsValid() )
			return;

		BodyRenderer.Set( "holdtype", (int)Citizen.CitizenAnimationHelper.HoldTypes.None );
		BodyRenderer.Set( "holdtype_handedness", (int)Citizen.CitizenAnimationHelper.Hand.Both );
	}

	static Transform CreateGripTransform( Vector3 position, Rotation rotation )
	{
		var transform = default( Transform );
		transform.Position = position;
		transform.Rotation = rotation;
		transform.Scale = Vector3.One;
		return transform;
	}

	float GetSwingVisualAmount()
	{
		if ( SwingVisualDuration <= 0f || TimeSinceSwingVisual >= SwingVisualDuration )
			return 0f;

		var progress = (TimeSinceSwingVisual / SwingVisualDuration).Clamp( 0f, 1f );
		return (float)Math.Sin( progress * Math.PI );
	}

	Rotation GetSwingVisualRotation()
	{
		if ( SwingVisualDuration <= 0f || TimeSinceSwingVisual >= SwingVisualDuration )
			return Rotation.Identity;

		var progress = (TimeSinceSwingVisual / SwingVisualDuration).Clamp( 0f, 1f );
		var easedProgress = progress * progress * (3f - 2f * progress);
		var pitch = SwingVisualStartAngles.pitch + (SwingVisualEndAngles.pitch - SwingVisualStartAngles.pitch) * easedProgress;
		var yaw = SwingVisualStartAngles.yaw + (SwingVisualEndAngles.yaw - SwingVisualStartAngles.yaw) * easedProgress;
		var roll = SwingVisualStartAngles.roll + (SwingVisualEndAngles.roll - SwingVisualStartAngles.roll) * easedProgress;
		return new Angles( pitch, yaw, roll ).ToRotation();
	}

	void SetVisible( bool visible )
	{
		if ( ClubObject.IsValid() )
		{
			ClubObject.Enabled = visible;
		}
	}

	void ClearIk()
	{
		if ( !BodyRenderer.IsValid() )
			return;

		BodyRenderer.ClearIk( "hand_right" );
		BodyRenderer.ClearIk( "hand_left" );
	}

	protected override void OnDestroy()
	{
		if ( OwnsClubObject && ClubObject.IsValid() )
		{
			ClubObject.Destroy();
		}
	}
}
