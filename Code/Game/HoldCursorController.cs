namespace Sandbox;

[Title( "Hold Cursor Controller" )]
[Category( "Battle Royale" )]
public sealed class HoldCursorController : Component
{
	[Property] public string HoldAction { get; set; } = "Score";

	protected override void OnUpdate()
	{
		if ( !GameObject.Network.IsOwner )
			return;

		Mouse.Visibility = Input.Down( HoldAction )
			? MouseVisibility.Visible
			: MouseVisibility.Hidden;
	}
}
