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

		if ( !Input.Pressed( "Attack1" ) )
			return;

		Character.EnsureCharacterComponents();

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
}
