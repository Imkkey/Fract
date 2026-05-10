using System;

namespace Sandbox;

[Title( "Valtryek Volt Slide Visual" )]
[Category( "Battle Royale" )]
public sealed class ValtryekVoltSlideVisual : Component
{
	public GameObject SourceObject { get; set; }
	public Vector3 StartPosition { get; set; }
	public Vector3 EndPosition { get; set; }
	public float Duration { get; set; } = 0.24f;
	public int Seed { get; set; }

	public float ArcInterval { get; set; } = 0.035f;
	public int ArcsPerPulse { get; set; } = 3;
	public float ArcRadius { get; set; } = 34f;
	public float ArcHeight { get; set; } = 24f;
	public float ArcLifetime { get; set; } = 0.16f;
	public float CoreScale { get; set; } = 0.5f;
	public float GlowScale { get; set; } = 1.65f;
	public Color CoreColor { get; set; } = new( 1f, 1f, 1f, 1f );
	public Color GlowColor { get; set; } = new( 0.04f, 0.48f, 1f, 0.8f );

	TimeUntil LifeTime { get; set; }
	TimeUntil NextArcTime { get; set; }
	Vector3 PreviousPosition { get; set; }
	Random Rng { get; set; }
	PointLight SlideLight { get; set; }

	protected override void OnStart()
	{
		LifeTime = Duration + 0.16f;
		NextArcTime = 0f;
		PreviousPosition = GetSourcePosition();
		Rng = new Random( Seed );
		CreateSlideLight();
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f || !SourceObject.IsValid() )
		{
			GameObject.Destroy();
			return;
		}

		var currentPosition = GetSourcePosition();
		UpdateSlideLight( currentPosition );

		if ( NextArcTime <= 0f )
		{
			SpawnArcPulse( PreviousPosition, currentPosition );
			PreviousPosition = currentPosition;
			NextArcTime = ArcInterval;
		}
	}

	Vector3 GetSourcePosition()
	{
		if ( SourceObject.IsValid() )
			return SourceObject.WorldPosition + Vector3.Up * 28f;

		var progress = Duration <= 0f ? 1f : (1f - LifeTime / Duration).Clamp( 0f, 1f );
		return Vector3.Lerp( StartPosition, EndPosition, progress ) + Vector3.Up * 28f;
	}

	void SpawnArcPulse( Vector3 from, Vector3 to )
	{
		if ( (to - from).Length <= 1f )
			to = from + GetSlideDirection() * 18f;

		var direction = (to - from).WithZ( 0f ).Normal;
		if ( direction.Length <= 0.01f )
			direction = GetSlideDirection();

		var right = Vector3.Cross( direction, Vector3.Up ).Normal;
		if ( right.Length <= 0.01f )
			right = Vector3.Right;

		for ( var i = 0; i < ArcsPerPulse; i++ )
		{
			var sideA = RandomRange( -ArcRadius, ArcRadius );
			var sideB = RandomRange( -ArcRadius, ArcRadius );
			var heightA = RandomRange( -8f, ArcHeight );
			var heightB = RandomRange( -8f, ArcHeight );
			var start = from + right * sideA + Vector3.Up * heightA;
			var end = to + right * sideB + Vector3.Up * heightB;

			CreateBeam( start, end, GlowScale, GlowColor, 2.1f, "Volt Slide Glow" );
			CreateBeam( start, end, CoreScale, CoreColor, 4.4f, "Volt Slide Core" );
		}
	}

	Vector3 GetSlideDirection()
	{
		var direction = (EndPosition - StartPosition).WithZ( 0f ).Normal;
		return direction.Length > 0.01f ? direction : Vector3.Forward;
	}

	void CreateBeam( Vector3 start, Vector3 end, float scale, Color color, float brightness, string name )
	{
		var segment = new GameObject( GameObject, false, name );
		segment.NetworkMode = NetworkMode.Never;
		segment.WorldPosition = start;
		if ( (end - start).Length > 0.01f )
			segment.WorldRotation = Rotation.LookAt( (end - start).Normal, Vector3.Up );
		segment.Enabled = true;

		var target = new GameObject( GameObject, false, $"{name} Target" );
		target.NetworkMode = NetworkMode.Never;
		target.WorldPosition = end;
		target.Enabled = true;

		var beam = segment.Components.Create<BeamEffect>();
		beam.TargetGameObject = target;
		beam.TargetPosition = end;
		beam.BeamLifetime = ArcLifetime;
		beam.MaxBeams = 1;
		beam.InitialBurst = 1;
		beam.Looped = false;
		beam.FollowPoints = true;
		beam.Additive = true;
		beam.Lighting = false;
		beam.Shadows = false;
		beam.Opaque = false;
		beam.Scale = scale;
		beam.Alpha = color.a;
		beam.Brightness = brightness;
		beam.TextureScrollSpeed = 22f;
		beam.TextureScale = 0.018f;
		beam.SpawnBeam();
	}

	void CreateSlideLight()
	{
		SlideLight = GameObject.Components.Create<PointLight>();
		SlideLight.LightColor = new Color( 0.06f, 0.5f, 1f, 0.75f );
		SlideLight.Radius = 145f;
		SlideLight.Shadows = false;
	}

	void UpdateSlideLight( Vector3 position )
	{
		GameObject.WorldPosition = position;
	}

	float RandomRange( float min, float max )
	{
		return min + (float)Rng.NextDouble() * (max - min);
	}
}
