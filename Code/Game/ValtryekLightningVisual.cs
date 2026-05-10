namespace Sandbox;

[Title( "Valtryek Lightning Visual" )]
[Category( "Battle Royale" )]
public sealed class ValtryekLightningVisual : Component
{
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public Citizen.CitizenAnimationHelper.HoldTypes IdleHoldType { get; set; } = Citizen.CitizenAnimationHelper.HoldTypes.Pistol;
	[Property] public Citizen.CitizenAnimationHelper.Hand IdleHandedness { get; set; } = Citizen.CitizenAnimationHelper.Hand.Right;
	[Property] public Citizen.CitizenAnimationHelper.HoldTypes AttackHoldType { get; set; } = Citizen.CitizenAnimationHelper.HoldTypes.Punch;
	[Property] public Citizen.CitizenAnimationHelper.Hand AttackHandedness { get; set; } = Citizen.CitizenAnimationHelper.Hand.Right;
	[Property] public float AttackPoseSeconds { get; set; } = 0.22f;
	[Property] public float AttackValue { get; set; } = 1f;

	PlayerCharacter Character { get; set; }
	PlayerCombat Combat { get; set; }
	TimeUntil AttackPoseTime { get; set; }
	bool WasActive { get; set; }

	protected override void OnStart()
	{
		Character = GetComponent<PlayerCharacter>();
		Combat = GetComponent<PlayerCombat>();
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
	}

	protected override void OnUpdate()
	{
		if ( !ShouldUseValtryekPose() )
		{
			if ( WasActive )
			{
				ClearHoldAnimation();
				WasActive = false;
			}

			return;
		}

		WasActive = true;
		ApplyCurrentHoldAnimation();
	}

	protected override void OnPreRender()
	{
		if ( ShouldUseValtryekPose() )
			ApplyCurrentHoldAnimation();
	}

	public void PlayAttackVisual()
	{
		if ( !BodyRenderer.IsValid() )
			return;

		AttackPoseTime = AttackPoseSeconds;
		ApplyHoldAnimation( AttackHoldType, AttackHandedness );
		BodyRenderer.Set( "holdtype_attack", AttackValue );
		BodyRenderer.Set( "b_attack", true );
	}

	void ApplyCurrentHoldAnimation()
	{
		if ( AttackPoseTime > 0f )
		{
			ApplyHoldAnimation( AttackHoldType, AttackHandedness );
			return;
		}

		ApplyHoldAnimation( IdleHoldType, IdleHandedness );
	}

	void ApplyHoldAnimation( Citizen.CitizenAnimationHelper.HoldTypes holdType, Citizen.CitizenAnimationHelper.Hand handedness )
	{
		if ( !BodyRenderer.IsValid() )
			return;

		BodyRenderer.Set( "holdtype", (int)holdType );
		BodyRenderer.Set( "holdtype_handedness", (int)handedness );
	}

	void ClearHoldAnimation()
	{
		if ( !BodyRenderer.IsValid() )
			return;

		BodyRenderer.Set( "holdtype", (int)Citizen.CitizenAnimationHelper.HoldTypes.None );
		BodyRenderer.Set( "holdtype_handedness", (int)Citizen.CitizenAnimationHelper.Hand.Right );
	}

	bool ShouldUseValtryekPose()
	{
		return Character.IsValid()
			&& Character.CurrentCharacter == CharacterId.Valtryek
			&& (!Combat.IsValid() || !Combat.IsDead);
	}
}
