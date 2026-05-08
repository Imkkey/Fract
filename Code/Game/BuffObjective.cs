namespace Sandbox;

[Title( "Buff Objective" )]
[Category( "Battle Royale" )]
public sealed class BuffObjective : Component, Component.ITriggerListener
{
	[Property] public BuffType BuffType { get; set; } = BuffType.PhysicalDamagePercent;
	[Property] public float Value { get; set; } = 15f;
	[Property] public float RespawnSeconds { get; set; } = 20f;

	[Sync( Flags = SyncFlags.FromHost )] public bool Available { get; private set; } = true;

	TimeUntil RespawnTime { get; set; }

	protected override void OnUpdate()
	{
		SetRenderersEnabled( Available );

		if ( !Networking.IsHost )
			return;

		if ( !Available && RespawnTime <= 0f )
		{
			Available = true;
			SetRenderersEnabled( true );
		}
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( !Networking.IsHost || !Available )
			return;

		var combat = other.GameObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
		if ( !combat.IsValid() || combat.IsDead )
			return;

		combat.ApplyBuff( BuffType, Value );
		Available = false;
		RespawnTime = RespawnSeconds;
		SetRenderersEnabled( false );
	}

	public void OnTriggerExit( Collider other )
	{
	}

	void SetRenderersEnabled( bool enabled )
	{
		foreach ( var renderer in GameObject.Components.GetAll<ModelRenderer>( FindMode.Enabled | FindMode.Disabled | FindMode.InSelf | FindMode.InDescendants ) )
		{
			renderer.Enabled = enabled;
		}
	}
}
