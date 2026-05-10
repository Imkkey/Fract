namespace Sandbox;

[Title( "Player Attack Controller" )]
[Category( "Battle Royale" )]
public sealed class PlayerAttackController : Component
{
	PlayerCombat Combat { get; set; }
	PlayerCharacter Character { get; set; }

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Character = GetComponent<PlayerCharacter>();
	}

	protected override void OnUpdate()
	{
		if ( !GameObject.Network.IsOwner )
			return;

		if ( !Character.IsValid() || !Character.HasSelectedCharacter )
		{
			if ( Character.IsValid() )
			{
				if ( Input.Pressed( "Slot1" ) )
				{
					Character.RequestSelectCharacter( CharacterId.Cardveil );
				}
				else if ( Input.Pressed( "Slot2" ) )
				{
					Character.RequestSelectCharacter( CharacterId.ClubBrawler );
				}
				else if ( Input.Pressed( "Slot3" ) )
				{
					Character.RequestSelectCharacter( CharacterId.Mycell );
				}
				else if ( Input.Pressed( "Slot4" ) )
				{
					Character.RequestSelectCharacter( CharacterId.Valtryek );
				}
			}

			return;
		}

		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( Input.Down( "Score" ) )
			return;

		Character.EnsureCharacterComponents();

		if ( Input.Down( "Attack1" ) )
		{
			switch ( Character.CurrentCharacter )
			{
				case CharacterId.Cardveil:
					GetComponent<CardAttack>()?.TryAttack();
					break;
				case CharacterId.ClubBrawler:
					GetComponent<ClubAttack>()?.TryAttack();
					break;
				case CharacterId.Mycell:
					GetComponent<MycellAttack>()?.TryAttack();
					break;
				case CharacterId.Valtryek:
					GetComponent<ValtryekLightningAttack>()?.TryAttack();
					break;
			}
		}

		if ( Input.Pressed( "Ability1" ) )
		{
			switch ( Character.CurrentCharacter )
			{
				case CharacterId.Cardveil:
					GetComponent<CardAttack>()?.TryShuffleDash();
					break;
				case CharacterId.Mycell:
					GetComponent<MycellAttack>()?.TryMushroomStep();
					break;
			}
		}

		if ( Input.Pressed( "Ability2" ) )
		{
			switch ( Character.CurrentCharacter )
			{
				case CharacterId.Mycell:
					GetComponent<MycellAttack>()?.TryRottenHeal();
					break;
			}
		}

		if ( Input.Pressed( "Reload" ) )
		{
			switch ( Character.CurrentCharacter )
			{
				case CharacterId.Cardveil:
					GetComponent<CardAttack>()?.TryCycleLoadedHand();
					break;
				case CharacterId.Mycell:
					GetComponent<MycellAttack>()?.TrySporePit();
					break;
			}
		}
	}
}
