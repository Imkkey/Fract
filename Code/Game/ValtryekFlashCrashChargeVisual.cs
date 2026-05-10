using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Valtryek Flash Crash Charge Visual" )]
[Category( "Battle Royale" )]
public sealed class ValtryekFlashCrashChargeVisual : Component
{
	[Property] public int ChargeArcCount { get; set; } = 10;
	[Property] public int ReadyChargeArcCount { get; set; } = 14;
	[Property] public float ArcRefreshInterval { get; set; } = 0.045f;
	[Property] public float ChargeArcRadius { get; set; } = 24f;
	[Property] public float ReadyArcRadius { get; set; } = 42f;
	[Property] public float CoreScale { get; set; } = 0.18f;
	[Property] public float GlowScale { get; set; } = 0.72f;

	public GameObject SourceObject { get; set; }
	public string SourceBoneNames { get; set; }
	public Vector3 SourceBoneLocalOffset { get; set; }
	public Vector3 SourceFallbackWorldOffset { get; set; }
	public float RequiredChargeTime { get; set; } = 1f;
	public float Duration { get; set; } = 1f;

	PointLight ChargeLight { get; set; }
	TimeUntil LifeTime { get; set; }
	TimeUntil NextArcRefreshTime { get; set; }
	TimeSince Age { get; set; }
	Random Rng { get; set; }
	List<ValtryekPooledLightningArc> Arcs { get; } = new();

	protected override void OnStart()
	{
		LifeTime = Duration;
		NextArcRefreshTime = 0f;
		Age = 0f;
		Rng = new Random( GameObject.GetHashCode() );

		ChargeLight = GameObject.Components.Create<PointLight>();
		ChargeLight.LightColor = new Color( 0.08f, 0.52f, 1f, 0.85f );
		ChargeLight.Radius = 130f;
		ChargeLight.Shadows = false;

		CreateArcPool( Math.Max( ChargeArcCount, ReadyChargeArcCount ) );
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f || !SourceObject.IsValid() )
		{
			GameObject.Destroy();
			return;
		}

		var position = GetSourcePosition();
		WorldPosition = position;

		var progress = RequiredChargeTime <= 0f ? 1f : (Age / RequiredChargeTime).Clamp( 0f, 1f );
		var ready = progress >= 1f;
		var pulse = 1f + MathF.Sin( Time.Now * 38f ) * 0.12f;
		if ( ChargeLight.IsValid() )
			ChargeLight.Radius = (115f + progress * 115f) * pulse;

		if ( NextArcRefreshTime <= 0f )
		{
			NextArcRefreshTime = ArcRefreshInterval;
			RefreshArcs( position, progress, ready );
		}
	}

	void CreateArcPool( int count )
	{
		for ( var i = 0; i < count; i++ )
		{
			var arcObject = new GameObject( GameObject, false, $"Flash Crash Held Arc {i}" );
			arcObject.NetworkMode = NetworkMode.Never;
			arcObject.Enabled = true;

			var arc = arcObject.Components.Create<ValtryekPooledLightningArc>();
			arc.PointCount = 6;
			arc.Chaos = 14f;
			arc.CoreScale = CoreScale;
			arc.GlowScale = GlowScale;
			arc.Lifetime = Duration + 1f;
			arc.CoreColor = Color.White.WithAlpha( 0.94f );
			arc.GlowColor = new Color( 0.05f, 0.46f, 1f, 0.82f );
			arc.CoreBrightness = 5.1f;
			arc.GlowBrightness = 2.55f;
			arc.TendrilCoreScaleMultiplier = 0.4f;
			arc.TendrilGlowScaleMultiplier = 0.32f;
			arc.TendrilCoreAlphaMultiplier = 0.55f;
			arc.TendrilGlowAlphaMultiplier = 0.42f;
			arc.TendrilCoreBrightness = 3.1f;
			arc.TendrilGlowBrightness = 1.35f;
			arc.IncludeTendril = true;
			Arcs.Add( arc );
		}
	}

	void RefreshArcs( Vector3 position, float progress, bool ready )
	{
		var activeCount = ready ? ReadyChargeArcCount : ChargeArcCount;
		var radius = ready ? ReadyArcRadius : ChargeArcRadius + (ReadyArcRadius - ChargeArcRadius) * progress;

		for ( var i = 0; i < Arcs.Count; i++ )
		{
			var active = i < activeCount;
			Arcs[i].SetActive( active );
			if ( !active )
				continue;

			var start = position + RandomUnitVector() * radius;
			var end = position + RandomUnitVector() * radius;
			Arcs[i].Chaos = ready ? 15f : 9f + progress * 5f;
			Arcs[i].UpdateArc( start, end, Rng.Next( 0, int.MaxValue ) );
		}
	}

	Vector3 GetSourcePosition()
	{
		var bone = GetSourceBoneObject();
		if ( bone.IsValid() )
			return bone.WorldPosition + bone.WorldRotation * SourceBoneLocalOffset;

		return SourceObject.WorldPosition
			+ SourceObject.WorldRotation.Forward * SourceFallbackWorldOffset.x
			+ SourceObject.WorldRotation.Right * SourceFallbackWorldOffset.y
			+ Vector3.Up * SourceFallbackWorldOffset.z;
	}

	GameObject GetSourceBoneObject()
	{
		var renderer = SourceObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		if ( !renderer.IsValid() || string.IsNullOrWhiteSpace( SourceBoneNames ) )
			return null;

		foreach ( var rawName in SourceBoneNames.Split( ',', StringSplitOptions.RemoveEmptyEntries ) )
		{
			var name = rawName.Trim();
			var bone = renderer.GetBoneObject( name );
			if ( bone.IsValid() )
				return bone;
		}

		return null;
	}

	Vector3 RandomUnitVector()
	{
		return new Vector3(
			RandomRange( -1f, 1f ),
			RandomRange( -1f, 1f ),
			RandomRange( -0.35f, 1f )
		).Normal;
	}

	float RandomRange( float min, float max )
	{
		return min + (float)Rng.NextDouble() * (max - min);
	}

}
