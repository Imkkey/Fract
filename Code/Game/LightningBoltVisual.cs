using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Lightning Bolt Visual" )]
[Category( "Battle Royale" )]
public sealed class LightningBoltVisual : Component
{
	public Vector3 Start { get; set; }
	public Vector3 End { get; set; }
	public GameObject SourceObject { get; set; }
	public string SourceBoneNames { get; set; }
	public Vector3 SourceBoneLocalOffset { get; set; }
	public Vector3 SourceFallbackWorldOffset { get; set; }
	public int Seed { get; set; }
	public int PointCount { get; set; } = 13;
	public float Chaos { get; set; } = 46f;
	public float Lifetime { get; set; } = 0.22f;
	public float CoreScale { get; set; } = 0.58f;
	public float GlowScale { get; set; } = 2.05f;
	public Color CoreColor { get; set; } = new( 1f, 1f, 1f, 1f );
	public Color GlowColor { get; set; } = new( 0.05f, 0.46f, 1f, 0.86f );
	public float SourceFollowScale { get; set; } = 0.72f;
	public bool EnableFloorLightSpill { get; set; } = true;
	public int FloorLightCount { get; set; } = 3;
	public float FloorLightRadius { get; set; } = 170f;
	public float FloorLightAlpha { get; set; } = 0.34f;
	public List<Vector3> BranchStarts { get; set; } = new();
	public List<Vector3> BranchEnds { get; set; } = new();

	TimeUntil LifeTime { get; set; }
	GameObject SourceFollowStart { get; set; }
	GameObject SourceFollowEnd { get; set; }
	GameObject SourceFollowGlowStart { get; set; }
	GameObject SourceFollowGlowEnd { get; set; }

	protected override void OnStart()
	{
		LifeTime = Lifetime;
		DrawLightning();
		CreateSourceFollowBeam();
		CreateImpactFlash( End, 120f, 0.8f );
	}

	protected override void OnUpdate()
	{
		UpdateSourceFollowBeam();

		if ( LifeTime <= 0f )
		{
			GameObject.Destroy();
			return;
		}
	}

	void DrawLightning()
	{
		DrawBolt( Start, End, PointCount, Chaos, Seed, CoreScale, GlowScale, 1 );
		CreateFloorLightSpillAlongBolt();

		for ( var i = 0; i < BranchStarts.Count && i < BranchEnds.Count; i++ )
		{
			DrawBolt( BranchStarts[i], BranchEnds[i], 6, Chaos * 0.38f, Seed + 97 + i * 31, CoreScale * 0.6f, GlowScale * 0.46f, 0 );
			CreateImpactFlash( BranchEnds[i], 70f, 0.55f );
		}
	}

	void DrawBolt( Vector3 start, Vector3 end, int pointCount, float chaos, int seed, float coreScale, float glowScale, int extraTendrils )
	{
		DrawBoltPath( GenerateLightningPath( start, end, pointCount, chaos, seed ), coreScale, glowScale, 1f, true );

		for ( var i = 0; i < extraTendrils; i++ )
		{
			var tendrilPath = GenerateLightningPath( start, end, Math.Max( 5, pointCount - 5 ), chaos * 0.52f, seed + 17 + i * 53 );
			DrawBoltPath( tendrilPath, coreScale * 0.42f, glowScale * 0.34f, 0.52f, false );
		}
	}

	void DrawBoltPath( List<Vector3> path, float coreScale, float glowScale, float alphaMultiplier, bool includeGlow )
	{
		for ( var i = 0; i < path.Count - 1; i++ )
		{
			if ( includeGlow )
				CreateBeamSegment( path[i], path[i + 1], glowScale, GlowColor.WithAlpha( GlowColor.a * alphaMultiplier ), 2.6f, "Lightning Glow" );

			CreateBeamSegment( path[i], path[i + 1], coreScale, CoreColor.WithAlpha( CoreColor.a * alphaMultiplier ), 5.2f, "Lightning Core" );
		}
	}

	void CreateBeamSegment( Vector3 start, Vector3 end, float scale, Color color, float brightness, string name )
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
		beam.TextureScrollSpeed = 18f;
		beam.TextureScale = 0.018f;
		beam.SpawnBeam();
	}

	void CreateSourceFollowBeam()
	{
		if ( !SourceObject.IsValid() )
			return;

		var sourcePosition = GetSourcePosition();
		if ( (sourcePosition - Start).Length <= 1f )
			return;

		SourceFollowGlowStart = CreateFollowBeamSegment( sourcePosition, Start, GlowScale * SourceFollowScale, GlowColor, 2.2f, "Lightning Source Glow", out var glowEnd );
		SourceFollowGlowEnd = glowEnd;

		SourceFollowStart = CreateFollowBeamSegment( sourcePosition, Start, CoreScale * SourceFollowScale, CoreColor, 4.6f, "Lightning Source Core", out var coreEnd );
		SourceFollowEnd = coreEnd;
	}

	GameObject CreateFollowBeamSegment( Vector3 start, Vector3 end, float scale, Color color, float brightness, string name, out GameObject target )
	{
		var segment = new GameObject( GameObject, false, name );
		segment.NetworkMode = NetworkMode.Never;
		segment.WorldPosition = start;
		segment.Enabled = true;

		target = new GameObject( GameObject, false, $"{name} Target" );
		target.NetworkMode = NetworkMode.Never;
		target.WorldPosition = end;
		target.Enabled = true;

		var beam = segment.Components.Create<BeamEffect>();
		beam.TargetGameObject = target;
		beam.TargetPosition = end;
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
		beam.TextureScrollSpeed = 20f;
		beam.TextureScale = 0.018f;
		beam.SpawnBeam();

		return segment;
	}

	void UpdateSourceFollowBeam()
	{
		if ( !SourceObject.IsValid() )
			return;

		var sourcePosition = GetSourcePosition();
		UpdateFollowEndpoint( SourceFollowStart, SourceFollowEnd, sourcePosition );
		UpdateFollowEndpoint( SourceFollowGlowStart, SourceFollowGlowEnd, sourcePosition );
	}

	void UpdateFollowEndpoint( GameObject segment, GameObject target, Vector3 sourcePosition )
	{
		if ( !segment.IsValid() || !target.IsValid() )
			return;

		segment.WorldPosition = sourcePosition;
		target.WorldPosition = Start;
		if ( (Start - sourcePosition).Length > 0.01f )
			segment.WorldRotation = Rotation.LookAt( (Start - sourcePosition).Normal, Vector3.Up );
	}

	Vector3 GetSourcePosition()
	{
		var boneObject = GetSourceBoneObject();
		if ( boneObject.IsValid() )
			return boneObject.WorldPosition + boneObject.WorldRotation * SourceBoneLocalOffset;

		if ( SourceObject.IsValid() )
		{
			return SourceObject.WorldPosition
				+ SourceObject.WorldRotation.Forward * SourceFallbackWorldOffset.x
				+ SourceObject.WorldRotation.Right * SourceFallbackWorldOffset.y
				+ Vector3.Up * SourceFallbackWorldOffset.z;
		}

		return Start;
	}

	GameObject GetSourceBoneObject()
	{
		if ( !SourceObject.IsValid() || string.IsNullOrWhiteSpace( SourceBoneNames ) )
			return null;

		var bodyRenderer = SourceObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		if ( !bodyRenderer.IsValid() )
			return null;

		var names = SourceBoneNames.Split( new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries );
		foreach ( var rawName in names )
		{
			var boneName = rawName.Trim();
			if ( string.IsNullOrWhiteSpace( boneName ) )
				continue;

			var boneObject = bodyRenderer.GetBoneObject( boneName );
			if ( boneObject.IsValid() )
				return boneObject;
		}

		return null;
	}

	void CreateImpactFlash( Vector3 position, float radius, float alpha )
	{
		var flash = new GameObject( GameObject, false, "Lightning Impact Flash" );
		flash.NetworkMode = NetworkMode.Never;
		flash.WorldPosition = position;
		flash.Enabled = true;

		var light = flash.Components.Create<PointLight>();
		light.LightColor = new Color( 0.18f, 0.58f, 1f, alpha );
		light.Radius = radius * 1.35f;
		light.Shadows = false;
	}

	void CreateFloorLightSpillAlongBolt()
	{
		if ( !EnableFloorLightSpill || FloorLightCount <= 0 )
			return;

		var count = FloorLightCount.Clamp( 1, 6 );
		for ( var i = 0; i < count; i++ )
		{
			var t = (i + 1f) / (count + 1f);
			var boltPosition = Vector3.Lerp( Start, End, t );
			var floorPosition = GetFloorPositionBelow( boltPosition );
			CreateFloorLightSpill( floorPosition, FloorLightRadius, FloorLightAlpha );
		}
	}

	Vector3 GetFloorPositionBelow( Vector3 position )
	{
		var trace = Scene.Trace
			.Ray( position + Vector3.Up * 24f, position + Vector3.Down * 260f )
			.WithoutTags( "trigger", "deadplayer" )
			.Run();

		return trace.Hit ? trace.HitPosition + Vector3.Up * 6f : position;
	}

	void CreateFloorLightSpill( Vector3 position, float radius, float alpha )
	{
		var spill = new GameObject( GameObject, false, "Lightning Floor Light Spill" );
		spill.NetworkMode = NetworkMode.Never;
		spill.WorldPosition = position;
		spill.Enabled = true;

		var light = spill.Components.Create<PointLight>();
		light.LightColor = new Color( 0.04f, 0.42f, 1f, alpha );
		light.Radius = radius;
		light.Shadows = false;
	}

	static List<Vector3> GenerateLightningPath( Vector3 start, Vector3 end, int pointCount, float chaos, int seed )
	{
		var points = new List<Vector3>();
		pointCount = pointCount.Clamp( 2, 32 );

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
}
