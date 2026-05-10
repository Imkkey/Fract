using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Valtryek Flash Crash Orb" )]
[Category( "Battle Royale" )]
public sealed class ValtryekFlashCrashOrb : Component
{
	[Property] public int OrbArcCount { get; set; } = 8;
	[Property] public float ArcRefreshInterval { get; set; } = 0.035f;
	[Property] public float OrbArcRadius { get; set; } = 38f;
	[Property] public float CoreScale { get; set; } = 0.2f;
	[Property] public float GlowScale { get; set; } = 0.86f;

	public ValtryekLightningAttack OwnerAttack { get; set; }
	public GameObject Source { get; set; }
	public Vector3 Direction { get; set; } = Vector3.Forward;
	public float Speed { get; set; } = 2800f;
	public float MaxDistance { get; set; } = 1050f;
	public float HitRadius { get; set; } = 28f;
	public float MaxLifetime { get; set; } = 1.1f;
	public Color CoreColor { get; set; } = new( 0.72f, 0.92f, 1f, 1f );
	public Color GlowColor { get; set; } = new( 0.06f, 0.48f, 1f, 0.82f );

	float TravelledDistance { get; set; }
	TimeUntil LifeTime { get; set; }
	TimeUntil NextArcRefreshTime { get; set; }
	PointLight OrbLight { get; set; }
	Random Rng { get; set; }
	List<ValtryekPooledLightningArc> Arcs { get; } = new();

	protected override void OnStart()
	{
		LifeTime = MaxLifetime;
		NextArcRefreshTime = 0f;
		Rng = new Random( GameObject.GetHashCode() );
		OrbLight = GameObject.Components.Get<PointLight>( FindMode.Enabled | FindMode.InSelf );
		CreateArcPool( OrbArcCount );
	}

	protected override void OnUpdate()
	{
		if ( LifeTime <= 0f )
		{
			GameObject.Destroy();
			return;
		}

		var direction = Direction.Length > 0.01f ? Direction.Normal : WorldRotation.Forward.Normal;
		var previous = WorldPosition;
		var step = Speed * Time.Delta;
		var next = previous + direction * step;

		if ( HandleTouch( previous, next ) )
			return;

		WorldPosition = next;
		WorldRotation = Rotation.LookAt( direction, Vector3.Up ) * Rotation.FromRoll( Time.Now * 720f );
		TravelledDistance += step;

		UpdateLight();
		UpdateArcs();

		if ( TravelledDistance >= MaxDistance )
			GameObject.Destroy();
	}

	void CreateArcPool( int count )
	{
		for ( var i = 0; i < count; i++ )
		{
			var arcObject = new GameObject( GameObject, false, $"Flash Crash Orb Arc {i}" );
			arcObject.NetworkMode = NetworkMode.Never;
			arcObject.Enabled = true;

			var arc = arcObject.Components.Create<ValtryekPooledLightningArc>();
			arc.PointCount = 6;
			arc.Chaos = 16f;
			arc.CoreScale = CoreScale;
			arc.GlowScale = GlowScale;
			arc.Lifetime = MaxLifetime + 0.25f;
			arc.CoreColor = CoreColor;
			arc.GlowColor = GlowColor;
			arc.CoreBrightness = 5.4f;
			arc.GlowBrightness = 2.75f;
			arc.TendrilCoreScaleMultiplier = 0.4f;
			arc.TendrilGlowScaleMultiplier = 0.32f;
			arc.TendrilCoreAlphaMultiplier = 0.55f;
			arc.TendrilGlowAlphaMultiplier = 0.42f;
			arc.TendrilCoreBrightness = 3.2f;
			arc.TendrilGlowBrightness = 1.45f;
			arc.IncludeTendril = true;
			Arcs.Add( arc );
		}
	}

	void UpdateLight()
	{
		if ( !OrbLight.IsValid() )
			return;

		var pulse = 1f + MathF.Sin( Time.Now * 45f ) * 0.18f;
		OrbLight.Radius = 150f * pulse;
	}

	void UpdateArcs()
	{
		if ( NextArcRefreshTime > 0f )
			return;

		NextArcRefreshTime = ArcRefreshInterval;
		for ( var i = 0; i < Arcs.Count; i++ )
		{
			var start = WorldPosition + GetRandomPerpendicular( Direction ) * RandomRange( OrbArcRadius * 0.35f, OrbArcRadius );
			var end = WorldPosition + GetRandomPerpendicular( Direction ) * RandomRange( OrbArcRadius * 0.35f, OrbArcRadius );
			Arcs[i].UpdateArc( start, end, Rng.Next( 0, int.MaxValue ) );
		}
	}

	bool HandleTouch( Vector3 previous, Vector3 next )
	{
		var hits = Scene.Trace
			.Sphere( HitRadius, previous, next )
			.IgnoreGameObjectHierarchy( Source )
			.WithoutTags( "trigger", "deadplayer" )
			.RunAll();

		foreach ( var hit in hits )
		{
			if ( !hit.Hit )
				continue;

			var hitObject = hit.Collider?.GameObject ?? hit.GameObject;
			if ( !hitObject.IsValid() )
				continue;

			if ( Networking.IsHost && TryFindCombat( hitObject, out var target ) )
			{
				OwnerAttack?.NotifyFlashCrashOrbHit( target, hit.HitPosition );
			}

			WorldPosition = hit.HitPosition;
			GameObject.Destroy();
			return true;
		}

		return false;
	}

	Vector3 GetRandomPerpendicular( Vector3 direction )
	{
		var forward = direction.Length > 0.01f ? direction.Normal : Vector3.Forward;
		var right = Vector3.Cross( forward, Vector3.Up );
		if ( right.Length <= 0.01f )
			right = Vector3.Right;

		var up = Vector3.Cross( right.Normal, forward ).Normal;
		return (right.Normal * RandomRange( -1f, 1f ) + up * RandomRange( -1f, 1f )).Normal;
	}

	float RandomRange( float min, float max )
	{
		return min + (float)Rng.NextDouble() * (max - min);
	}

	static bool TryFindCombat( GameObject hitObject, out PlayerCombat combat )
	{
		combat = hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
		return combat.IsValid();
	}

}
