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
		EnsureCharacterComponents();

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
		EnsureComponent<CardThrowVisual>();
		EnsureComponent<CardAttack>();
		EnsureComponent<ClubWeaponIk>();
		EnsureComponent<ClubAttack>();
		EnsureComponent<MycellThrowVisual>();
		EnsureComponent<MycellAttack>();
		EnsureComponent<ValtryekLightningVisual>();
		EnsureComponent<ValtryekLightningAttack>();

		var character = CurrentCharacter;
		if ( AppliedCharacter == character )
			return;

		AppliedCharacter = character;
		GetComponent<PlayerCombat>()?.ApplyCharacterStats( character );
	}

	[Rpc.Host]
	public void RequestSelectCharacter( CharacterId characterId )
	{
		if ( SelectedCharacter != CharacterId.None )
			return;

		if ( characterId is not CharacterId.Cardveil and not CharacterId.ClubBrawler and not CharacterId.Mycell and not CharacterId.Valtryek )
			return;

		SelectedCharacter = characterId;
		SetVisualCharacter( characterId );
	}

	[Rpc.Broadcast]
	void SetVisualCharacter( CharacterId characterId )
	{
		VisualCharacter = characterId;
	}

	void EnsureComponent<T>() where T : Component, new()
	{
		if ( !Components.Get<T>( FindMode.EverythingInSelf ).IsValid() )
		{
			Components.Create<T>();
		}
	}
}
