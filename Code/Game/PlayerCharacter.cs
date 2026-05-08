namespace Sandbox;

[Title( "Player Character" )]
[Category( "Battle Royale" )]
public sealed class PlayerCharacter : Component
{
	[Sync( Flags = SyncFlags.FromHost )] public CharacterId SelectedCharacter { get; private set; } = CharacterId.None;

	CharacterId VisualCharacter { get; set; } = CharacterId.None;
	CharacterId AppliedCharacter { get; set; } = CharacterId.None;

	public CharacterId CurrentCharacter => SelectedCharacter != CharacterId.None ? SelectedCharacter : VisualCharacter;
	public bool HasSelectedCharacter => CurrentCharacter != CharacterId.None;

	protected override void OnStart()
	{
		VisualCharacter = SelectedCharacter;
		RemoveCharacterComponents();
		AppliedCharacter = CharacterId.None;

		if ( Networking.IsHost )
		{
			SelectedCharacter = CharacterId.None;
			VisualCharacter = CharacterId.None;
		}
	}

	protected override void OnUpdate()
	{
		EnsureCharacterComponents();
	}

	public void EnsureCharacterComponents()
	{
		var character = CurrentCharacter;
		if ( AppliedCharacter == character )
			return;

		RemoveCharacterComponents();
		AppliedCharacter = CharacterId.None;

		switch ( character )
		{
			case CharacterId.Cardveil:
				Components.Create<CardThrowVisual>();
				Components.Create<CardAttack>();
				break;
			case CharacterId.ClubBrawler:
				Components.Create<ClubWeaponIk>();
				Components.Create<ClubAttack>();
				break;
		}

		AppliedCharacter = character;
	}

	[Rpc.Host]
	public void RequestSelectCharacter( CharacterId characterId )
	{
		if ( SelectedCharacter != CharacterId.None )
			return;

		if ( characterId is not CharacterId.Cardveil and not CharacterId.ClubBrawler )
			return;

		SelectedCharacter = characterId;
		SetVisualCharacter( characterId );
	}

	[Rpc.Broadcast]
	void SetVisualCharacter( CharacterId characterId )
	{
		VisualCharacter = characterId;
	}

	void RemoveCharacterComponents()
	{
		DestroyComponent<CardAttack>();
		DestroyComponent<CardThrowVisual>();
		DestroyComponent<ClubAttack>();
		DestroyComponent<ClubWeaponIk>();
	}

	void DestroyComponent<T>() where T : Component
	{
		foreach ( var component in Components.GetAll<T>( FindMode.EverythingInSelf ) )
		{
			component.Destroy();
		}
	}
}
