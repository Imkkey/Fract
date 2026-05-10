namespace Sandbox;

[Title( "Mycell Mycelium Patch" )]
[Category( "Battle Royale" )]
public sealed class MycellMyceliumPatch : Component
{
	public GameObject Source { get; set; }
	public float Radius { get; set; } = 105f;
	public float Duration { get; set; } = 6f;
	public Color DebugColor { get; set; } = new( 0.54f, 1f, 0.22f, 1f );

	TimeUntil LifeTime { get; set; }

	protected override void OnStart()
	{
		LifeTime = Duration;
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f )
		{
			GameObject.Destroy();
			return;
		}

		DrawPatch();
	}

	public bool Contains( Vector3 position )
	{
		return (position - WorldPosition).WithZ( 0f ).Length <= Radius;
	}

	void DrawPatch()
	{
		var alpha = Duration <= 0f ? 0.18f : (LifeTime / Duration).Clamp( 0.08f, 0.22f );
		var color = DebugColor.WithAlpha( alpha );
		var center = WorldPosition + Vector3.Up * 2f;
		var segments = 18;
		var previous = GetRingPoint( center, 0f );

		for ( var i = 1; i <= segments; i++ )
		{
			var angle = 360f * i / segments;
			var current = GetRingPoint( center, angle );
			DebugOverlay.Line( previous, current, color, Time.Delta * 2f, default( Transform ), false );
			previous = current;
		}
	}

	Vector3 GetRingPoint( Vector3 center, float angle )
	{
		var direction = Rotation.FromYaw( angle ) * Vector3.Forward;
		return center + direction.Normal * Radius;
	}
}
