namespace Sandbox;

using System;
using System.Collections.Generic;

[Title( "Valtryek Ground Lightning Visual" )]
[Category( "Battle Royale" )]
public sealed class ValtryekGroundLightningVisual : Component
{
	public Vector3 Start { get; set; }
	public Vector3 End { get; set; }
	public float Lifetime { get; set; } = 0.16f;
	public float CoreScale { get; set; } = 0.42f;
	public float GlowScale { get; set; } = 1.28f;
	public int PointCount { get; set; } = 5;
	public float Chaos { get; set; } = 22f;
	public float BranchChance { get; set; } = 0.35f;
	public float BranchLength { get; set; } = 58f;
	public float SecondaryBoltChance { get; set; } = 0.35f;
	public bool CreateLight { get; set; } = false;
	public int Seed { get; set; }

	TimeUntil LifeTime { get; set; }
	Random Rng { get; set; }

	protected override void OnStart()
	{
		LifeTime = Lifetime;
		Rng = new Random( Seed );
		CoreScale *= RandomRange( 0.82f, 1.18f );
		GlowScale *= RandomRange( 0.78f, 1.24f );
		Chaos *= RandomRange( 0.72f, 1.35f );
		DrawZigZagLightning();
		if ( CreateLight )
			CreateFloorLight();
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f )
			GameObject.Destroy();
	}

	void DrawZigZagLightning()
	{
		var points = GenerateGroundPath();
		DrawPath( points, CoreScale, GlowScale, 1f, true );

		if ( RandomRange( 0f, 1f ) <= SecondaryBoltChance )
		{
			var secondaryPoints = GenerateGroundPath();
			DrawPath( secondaryPoints, CoreScale * 0.42f, GlowScale * 0.36f, 0.58f, false );
		}

		if ( RandomRange( 0f, 1f ) <= BranchChance && points.Count > 2 )
		{
			var branchIndex = (int)RandomRange( 1f, points.Count - 1f );
			var branchStart = points[branchIndex.Clamp( 1, points.Count - 2 )];
			var direction = (End - Start).WithZ( 0f ).Normal;
			if ( direction.Length <= 0.01f )
				direction = Vector3.Forward;

			var side = Vector3.Cross( direction, Vector3.Up ).Normal;
			if ( side.Length <= 0.01f )
				side = Vector3.Right;

			side *= RandomRange( 0f, 1f ) > 0.5f ? 1f : -1f;
			var branchEnd = branchStart + side * RandomRange( BranchLength * 0.45f, BranchLength ) + direction * RandomRange( 8f, 28f );
			branchEnd.z = End.z + RandomRange( -2f, 8f );
			var branchMid = Vector3.Lerp( branchStart, branchEnd, 0.5f ) + side * RandomRange( -12f, 12f ) + Vector3.Up * RandomRange( -2f, 10f );
			CreateBeam( branchStart, branchMid, GlowScale * 0.5f, new Color( 0.05f, 0.45f, 1f, 0.55f ), 1.45f, "Ground Lightning Branch Glow" );
			CreateBeam( branchStart, branchMid, CoreScale * 0.5f, Color.White, 2.9f, "Ground Lightning Branch Core" );
			CreateBeam( branchMid, branchEnd, GlowScale * 0.42f, new Color( 0.05f, 0.45f, 1f, 0.48f ), 1.25f, "Ground Lightning Branch Glow" );
			CreateBeam( branchMid, branchEnd, CoreScale * 0.42f, Color.White, 2.5f, "Ground Lightning Branch Core" );
		}
	}

	void DrawPath( List<Vector3> points, float coreScale, float glowScale, float alphaMultiplier, bool includeGlow )
	{
		for ( var i = 0; i < points.Count - 1; i++ )
		{
			var brightness = RandomRange( 0.82f, 1.28f );
			if ( includeGlow )
				CreateBeam( points[i], points[i + 1], glowScale, new Color( 0.05f, 0.45f, 1f, 0.78f * alphaMultiplier ), 2.1f * brightness, "Ground Lightning Glow" );

			CreateBeam( points[i], points[i + 1], coreScale, Color.White.WithAlpha( alphaMultiplier ), 4.2f * brightness, "Ground Lightning Core" );
		}
	}

	void CreateBeam( Vector3 start, Vector3 end, float scale, Color color, float brightness, string name )
	{
		var beamObject = new GameObject( GameObject, false, name );
		beamObject.NetworkMode = NetworkMode.Never;
		beamObject.WorldPosition = start;
		if ( (end - start).Length > 0.01f )
			beamObject.WorldRotation = Rotation.LookAt( (end - start).Normal, Vector3.Up );
		beamObject.Enabled = true;

		var target = new GameObject( GameObject, false, $"{name} Target" );
		target.NetworkMode = NetworkMode.Never;
		target.WorldPosition = end;
		target.Enabled = true;

		var beam = beamObject.Components.Create<BeamEffect>();
		beam.TargetGameObject = target;
		beam.TargetPosition = Vector3.Zero;
		beam.BeamLifetime = Lifetime;
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
		beam.TextureScrollSpeed = 24f;
		beam.TextureScale = 0.018f;
		beam.SpawnBeam();
	}

	List<Vector3> GenerateGroundPath()
	{
		var points = new List<Vector3>();
		var count = PointCount.Clamp( 2, 8 );
		var direction = (End - Start).WithZ( 0f ).Normal;
		if ( direction.Length <= 0.01f )
			direction = Vector3.Forward;

		var right = Vector3.Cross( direction, Vector3.Up ).Normal;
		if ( right.Length <= 0.01f )
			right = Vector3.Right;

		for ( var i = 0; i < count; i++ )
		{
			var t = i / (count - 1f);
			var point = Vector3.Lerp( Start, End, t );
			var fade = (float)Math.Sin( t * Math.PI );
			point += right * RandomRange( -Chaos, Chaos ) * fade;
			point += Vector3.Up * RandomRange( -4f, 8f ) * fade;
			points.Add( point );
		}

		points[0] = Start;
		points[^1] = End;
		return points;
	}

	float RandomRange( float min, float max )
	{
		return min + (float)Rng.NextDouble() * (max - min);
	}

	void CreateFloorLight()
	{
		var light = GameObject.Components.Create<PointLight>();
		light.LightColor = new Color( 0.06f, 0.48f, 1f, 0.32f );
		light.Radius = 120f;
		light.Shadows = false;
	}
}
