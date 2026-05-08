namespace Sandbox;

[Title( "Card Throw Visual" )]
[Category( "Battle Royale" )]
public sealed class CardThrowVisual : Component
{
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public Model CardModel { get; set; }
	[Property] public string WeaponBoneName { get; set; } = "hold_R";
	[Property] public string FallbackHandBoneName { get; set; } = "hand_R";
	[Property] public Vector3 CardLocalOffset { get; set; } = new( 0f, 0f, 0f );
	[Property] public Angles CardLocalAngles { get; set; } = new( 90f, 0f, 0f );
	[Property] public Vector3 CardLocalScale { get; set; } = Vector3.One;
	[Property] public float HideCardsOnThrowSeconds { get; set; } = 0.22f;
	[Property] public Citizen.CitizenAnimationHelper.HoldTypes IdleHoldType { get; set; } = Citizen.CitizenAnimationHelper.HoldTypes.Pistol;
	[Property] public Citizen.CitizenAnimationHelper.Hand IdleHandedness { get; set; } = Citizen.CitizenAnimationHelper.Hand.Right;
	[Property] public Citizen.CitizenAnimationHelper.HoldTypes ThrowHoldType { get; set; } = Citizen.CitizenAnimationHelper.HoldTypes.Punch;
	[Property] public Citizen.CitizenAnimationHelper.Hand ThrowHandedness { get; set; } = Citizen.CitizenAnimationHelper.Hand.Right;
	[Property] public float PunchAttackValue { get; set; } = 1f;

	PlayerCharacter Character { get; set; }
	PlayerCombat Combat { get; set; }
	GameObject CardObject { get; set; }
	GameObject AttachedBoneObject { get; set; }
	TimeUntil ShowCardsTime { get; set; }

	protected override void OnStart()
	{
		Character = GetComponent<PlayerCharacter>();
		Combat = GetComponent<PlayerCombat>();
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		CardModel ??= Model.Load( "models/card.vmdl" );
		EnsureCardObject();
		SetCardsVisible( false );
	}

	protected override void OnUpdate()
	{
		if ( !ShouldUseCardPose() )
		{
			SetCardsVisible( false );
			return;
		}

		ApplyHoldAnimation( ShowCardsTime > 0f ? ThrowHoldType : IdleHoldType, ShowCardsTime > 0f ? ThrowHandedness : IdleHandedness );

		if ( !AttachCardsToHandBone() )
		{
			SetCardsVisible( false );
			return;
		}

		ApplyCardLocalTransform();
		SetCardsVisible( ShowCardsTime <= 0f );
	}

	protected override void OnPreRender()
	{
		if ( ShouldUseCardPose() )
		{
			ApplyHoldAnimation( ShowCardsTime > 0f ? ThrowHoldType : IdleHoldType, ShowCardsTime > 0f ? ThrowHandedness : IdleHandedness );
		}

		if ( !CardObject.IsValid() || !CardObject.Enabled )
			return;

		ApplyCardLocalTransform();
	}

	public void PlayThrowVisual()
	{
		if ( !BodyRenderer.IsValid() )
			return;

		ApplyHoldAnimation( ThrowHoldType, ThrowHandedness );
		ShowCardsTime = HideCardsOnThrowSeconds;
		SetCardsVisible( false );
		BodyRenderer.Set( "holdtype_attack", PunchAttackValue );
		BodyRenderer.Set( "b_attack", true );
	}

	void EnsureCardObject()
	{
		if ( CardObject.IsValid() )
			return;

		if ( !CardModel.IsValid() )
			return;

		CardObject = new GameObject( false, "Card Hand Visual" );
		CardObject.NetworkMode = NetworkMode.Never;

		var renderer = CardObject.Components.Create<ModelRenderer>();
		renderer.Model = CardModel;
	}

	bool AttachCardsToHandBone()
	{
		EnsureCardObject();

		if ( !BodyRenderer.IsValid() || !CardObject.IsValid() )
			return false;

		var boneObject = GetWeaponBoneObject();
		if ( !boneObject.IsValid() )
			return false;

		if ( AttachedBoneObject == boneObject )
			return true;

		CardObject.SetParent( boneObject, false );
		AttachedBoneObject = boneObject;
		ApplyCardLocalTransform();
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

	void ApplyCardLocalTransform()
	{
		if ( !CardObject.IsValid() )
			return;

		CardObject.LocalPosition = CardLocalOffset;
		CardObject.LocalRotation = CardLocalAngles.ToRotation();
		CardObject.LocalScale = CardLocalScale;
	}

	void ApplyHoldAnimation( Citizen.CitizenAnimationHelper.HoldTypes holdType, Citizen.CitizenAnimationHelper.Hand handedness )
	{
		if ( !BodyRenderer.IsValid() )
			return;

		BodyRenderer.Set( "holdtype", (int)holdType );
		BodyRenderer.Set( "holdtype_handedness", (int)handedness );
	}

	void SetCardsVisible( bool visible )
	{
		if ( CardObject.IsValid() )
		{
			CardObject.Enabled = visible;
		}
	}

	bool ShouldUseCardPose()
	{
		return Character.IsValid()
			&& Character.CurrentCharacter == CharacterId.CardThrower
			&& (!Combat.IsValid() || !Combat.IsDead);
	}

	protected override void OnDestroy()
	{
		if ( CardObject.IsValid() )
		{
			CardObject.Destroy();
		}
	}
}
