using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Valtryek Pooled Lightning Arc" )]
[Category( "Battle Royale" )]
public sealed class ValtryekPooledLightningArc : Component
{
	public int PointCount { get; set; } = 5;
	public float Chaos { get; set; } = 10f;
	public float CoreScale { get; set; } = 0.06f;
	public float GlowScale { get; set; } = 0.22f;
	public float Lifetime { get; set; } = 30f;
	public Color CoreColor { get; set; } = Color.White;
	public Color GlowColor { get; set; } = new( 0.05f, 0.46f, 1f, 0.78f );
	public float CoreBrightness { get; set; } = 5.2f;
	public float GlowBrightness { get; set; } = 2.6f;
	public float TendrilCoreScaleMultiplier { get; set; } = 0.42f;
	public float TendrilGlowScaleMultiplier { get; set; } = 0.34f;
	public float TendrilCoreAlphaMultiplier { get; set; } = 0.55f;
	public float TendrilGlowAlphaMultiplier { get; set; } = 0.42f;
	public float TendrilCoreBrightness { get; set; } = 3.1f;
	public float TendrilGlowBrightness { get; set; } = 1.4f;
	public bool IncludeTendril { get; set; } = true;

	List<BeamPair> MainSegments { get; } = new();
	List<BeamPair> TendrilSegments { get; } = new();
	int SegmentCount => Math.Max( 1, PointCount.Clamp( 2, 12 ) - 1 );

	protected override void OnStart()
	{
		CreateSegmentPool();
	}

	public void SetActive( bool active )
	{
		foreach ( var segment in MainSegments )
			SetSegmentActive( segment, active );

		foreach ( var segment in TendrilSegments )
			SetSegmentActive( segment, active && IncludeTendril );
	}

	public void UpdateArc( Vector3 start, Vector3 end, int seed )
	{
		if ( MainSegments.Count == 0 )
			CreateSegmentPool();

		var mainPath = GenerateLightningPath( start, end, PointCount, Chaos, seed );
		UpdatePath( MainSegments, mainPath );

		if ( IncludeTendril )
		{
			var tendrilPointCount = Math.Max( 3, PointCount - 1 );
			var tendrilPath = GenerateLightningPath( start, end, tendrilPointCount, Chaos * 0.58f, seed + 53 );
			UpdatePath( TendrilSegments, tendrilPath );
		}
	}

	void CreateSegmentPool()
	{
		if ( MainSegments.Count > 0 )
			return;

		for ( var i = 0; i < SegmentCount; i++ )
			MainSegments.Add( CreateSegmentPair( $"Main {i}", CoreScale, GlowScale, CoreColor, GlowColor, CoreBrightness, GlowBrightness ) );

		for ( var i = 0; i < SegmentCount; i++ )
			TendrilSegments.Add( CreateSegmentPair( $"Tendril {i}", CoreScale * TendrilCoreScaleMultiplier, GlowScale * TendrilGlowScaleMultiplier, CoreColor.WithAlpha( CoreColor.a * TendrilCoreAlphaMultiplier ), GlowColor.WithAlpha( GlowColor.a * TendrilGlowAlphaMultiplier ), TendrilCoreBrightness, TendrilGlowBrightness ) );
	}

	BeamPair CreateSegmentPair( string name, float coreScale, float glowScale, Color coreColor, Color glowColor, float coreBrightness, float glowBrightness )
	{
		return new BeamPair
		{
			Glow = CreateBeamSegment( $"{name} Glow", glowScale, glowColor, glowBrightness, out var glowTarget ),
			GlowTarget = glowTarget,
			Core = CreateBeamSegment( $"{name} Core", coreScale, coreColor, coreBrightness, out var coreTarget ),
			CoreTarget = coreTarget
		};
	}

	GameObject CreateBeamSegment( string name, float scale, Color color, float brightness, out GameObject target )
	{
		var segment = new GameObject( GameObject, false, name );
		segment.NetworkMode = NetworkMode.Never;
		segment.Enabled = true;

		target = new GameObject( GameObject, false, $"{name} Target" );
		target.NetworkMode = NetworkMode.Never;
		target.Enabled = true;

		var beam = segment.Components.Create<BeamEffect>();
		beam.TargetGameObject = target;
		beam.BeamLifetime = Lifetime;
		beam.MaxBeams = 1;
		beam.InitialBurst = 1;
		beam.Looped = true;
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

		return segment;
	}

	void UpdatePath( List<BeamPair> segments, List<Vector3> path )
	{
		for ( var i = 0; i < segments.Count; i++ )
		{
			var active = i < path.Count - 1;
			SetSegmentActive( segments[i], active );
			if ( !active )
				continue;

			UpdatePairEndpoint( segments[i], path[i], path[i + 1] );
		}
	}

	void UpdatePairEndpoint( BeamPair pair, Vector3 start, Vector3 end )
	{
		UpdateBeamEndpoint( pair.Glow, pair.GlowTarget, start, end );
		UpdateBeamEndpoint( pair.Core, pair.CoreTarget, start, end );
	}

	void UpdateBeamEndpoint( GameObject beamObject, GameObject target, Vector3 start, Vector3 end )
	{
		if ( !beamObject.IsValid() || !target.IsValid() )
			return;

		beamObject.WorldPosition = start;
		target.WorldPosition = end;
		if ( (end - start).Length > 0.01f )
			beamObject.WorldRotation = Rotation.LookAt( (end - start).Normal, Vector3.Up );
	}

	void SetSegmentActive( BeamPair pair, bool active )
	{
		if ( pair.Glow.IsValid() )
			pair.Glow.Enabled = active;
		if ( pair.GlowTarget.IsValid() )
			pair.GlowTarget.Enabled = active;
		if ( pair.Core.IsValid() )
			pair.Core.Enabled = active;
		if ( pair.CoreTarget.IsValid() )
			pair.CoreTarget.Enabled = active;
	}

	static List<Vector3> GenerateLightningPath( Vector3 start, Vector3 end, int pointCount, float chaos, int seed )
	{
		var points = new List<Vector3>();
		pointCount = pointCount.Clamp( 2, 12 );

		var direction = (end - start).Normal;
		if ( direction.Length <= 0.01f )
			direction = Vector3.Forward;

		var right = Vector3.Cross( direction, Vector3.Up );
		if ( right.Length <= 0.01f )
			right = Vector3.Right;
		right = right.Normal;

		var up = Vector3.Cross( right, direction ).Normal;
		var random = new Random( seed );

		for ( var i = 0; i < pointCount; i++ )
		{
			var t = i / (pointCount - 1f);
			var fade = (float)Math.Sin( t * Math.PI );
			var point = Vector3.Lerp( start, end, t );
			point += right * RandomRange( random, -chaos, chaos ) * fade;
			point += up * RandomRange( random, -chaos, chaos ) * fade;
			points.Add( point );
		}

		points[0] = start;
		points[^1] = end;
		return points;
	}

	static float RandomRange( Random random, float min, float max )
	{
		return min + (float)random.NextDouble() * (max - min);
	}

	struct BeamPair
	{
		public GameObject Glow;
		public GameObject GlowTarget;
		public GameObject Core;
		public GameObject CoreTarget;
	}
}
