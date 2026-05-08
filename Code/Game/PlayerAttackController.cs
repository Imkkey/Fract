namespace Sandbox;

[Title( "Player Attack Controller" )]
[Category( "Battle Royale" )]
public sealed class PlayerAttackController : Component
{
	PlayerCombat Combat { get; set; }
	PlayerCharacter Character { get; set; }
	CardAttack CardAttack { get; set; }
	ClubAttack ClubAttack { get; set; }

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Character = GetComponent<PlayerCharacter>();
		CardAttack = GetComponent<CardAttack>();
		ClubAttack = GetComponent<ClubAttack>();
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
					Character.RequestSelectCharacter( CharacterId.CardThrower );
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

		if ( !Input.Pressed( "Attack1" ) )
			return;

		switch ( Character.CurrentCharacter )
		{
			case CharacterId.CardThrower:
				CardAttack?.TryAttack();
				break;
			case CharacterId.ClubBrawler:
				ClubAttack?.TryAttack();
				break;
		}
	}
}
