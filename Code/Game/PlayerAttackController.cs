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
			}
		}

		if ( Input.Pressed( "Ability1" ) )
		{
			switch ( Character.CurrentCharacter )
			{
				case CharacterId.Cardveil:
					GetComponent<CardAttack>()?.TryShuffleDash();
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
			}
		}
	}
}
