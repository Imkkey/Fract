namespace Sandbox;

[Title( "Battle Royale Match" )]
[Category( "Battle Royale" )]
public sealed class BattleRoyaleMatch : Component
{
	public static BattleRoyaleMatch Current { get; private set; }

	[Sync( Flags = SyncFlags.FromHost )] public bool MatchFinished { get; private set; }
	[Sync( Flags = SyncFlags.FromHost )] public string WinnerName { get; private set; } = "";
	[Sync( Flags = SyncFlags.FromHost )] public int AlivePlayers { get; private set; }

	protected override void OnStart()
	{
		Current = this;

		if ( Networking.IsHost )
		{
			MatchFinished = false;
			WinnerName = "";
		}
	}

	protected override void OnDestroy()
	{
		if ( Current == this )
		{
			Current = null;
		}
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost || MatchFinished )
			return;

		UpdateAliveState();
	}

	public void NotifyPlayerDied( PlayerCombat player, GameObject killer )
	{
		if ( !Networking.IsHost || MatchFinished )
			return;

		UpdateAliveState();
	}

	void UpdateAliveState()
	{
		var alive = Scene.GetAllComponents<PlayerCombat>()
			.Where( player => player.IsValid() && player.CountsForMatch && !player.IsDead )
			.ToArray();

		AlivePlayers = alive.Length;

		if ( alive.Length == 1 && Scene.GetAllComponents<PlayerCombat>().Count( player => player.IsValid() && player.CountsForMatch ) > 1 )
		{
			MatchFinished = true;
			WinnerName = alive[0].GameObject.Network.Owner?.DisplayName ?? alive[0].GameObject.Name;
			Log.Info( $"Battle Royale winner: {WinnerName}" );
		}
	}
}
