namespace Sandbox;

[Title( "Player Character" )]
[Category( "Battle Royale" )]
public sealed class PlayerCharacter : Component
{
	[Sync( Flags = SyncFlags.FromHost )] public CharacterId SelectedCharacter { get; private set; } = CharacterId.None;

	CharacterId VisualCharacter { get; set; } = CharacterId.None;

	public CharacterId CurrentCharacter => SelectedCharacter != CharacterId.None ? SelectedCharacter : VisualCharacter;
	public bool HasSelectedCharacter => CurrentCharacter != CharacterId.None;

	protected override void OnStart()
	{
		VisualCharacter = SelectedCharacter;

		if ( Networking.IsHost )
		{
			SelectedCharacter = CharacterId.None;
			VisualCharacter = CharacterId.None;
		}
	}

	[Rpc.Host]
	public void RequestSelectCharacter( CharacterId characterId )
	{
		if ( SelectedCharacter != CharacterId.None )
			return;

		if ( characterId is not CharacterId.CardThrower and not CharacterId.ClubBrawler )
			return;

		SelectedCharacter = characterId;
		SetVisualCharacter( characterId );
	}

	[Rpc.Broadcast]
	void SetVisualCharacter( CharacterId characterId )
	{
		VisualCharacter = characterId;
	}
}
