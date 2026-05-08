using System;

namespace Sandbox;

[Title( "Card Projectile Visual" )]
[Category( "Battle Royale" )]
public sealed class CardProjectileVisual : Component
{
	[Property] public float Speed { get; set; } = 1800f;
	[Property] public Angles ModelFlightAngles { get; set; } = new( 0f, 0f, 0f );

	public Vector3 Direction { get; set; } = Vector3.Forward;
	public float MaxDistance { get; set; } = 900f;

	float TravelledDistance { get; set; }

	protected override void OnUpdate()
	{
		var direction = Direction.Length > 0.01f ? Direction.Normal : Vector3.Forward;
		var step = Speed * Time.Delta;
		TravelledDistance += step;
		WorldPosition += direction * step;

		var up = MathF.Abs( direction.Dot( Vector3.Up ) ) > 0.98f ? Vector3.Right : Vector3.Up;
		WorldRotation = Rotation.LookAt( direction, up ) * ModelFlightAngles.ToRotation();

		if ( TravelledDistance >= MaxDistance )
		{
			GameObject.Destroy();
		}
	}
}
