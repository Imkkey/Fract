using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Valtryek Lightning Attack" )]
[Category( "Battle Royale" )]
public sealed class ValtryekLightningAttack : Component
{
	[Property] public float Damage { get; set; } = 42f;
	[Property] public float Cooldown { get; set; } = 0.55f;
	[Property] public float Range { get; set; } = 1200f;
	[Property] public float BranchRadius { get; set; } = 140f;
	[Property] public int MaxBranchTargets { get; set; } = 2;
	[Property] public float BranchDamage { get; set; } = 14f;
	[Property] public float VisualLifetime { get; set; } = 0.22f;
	[Property] public int VisualPointCount { get; set; } = 13;
	[Property] public float VisualChaos { get; set; } = 46f;
	[Property] public float RagdollImpulse { get; set; } = 95f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 24f;
	[Property, Group( "Muzzle" )] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property, Group( "Muzzle" )] public string StartBoneNames { get; set; } = "finger_index_03_R,finger_index_02_R,finger_index_01_R,index_03_R,index_02_R,index_01_R,hold_R,hand_R";
	[Property, Group( "Muzzle" )] public Vector3 BoneLocalOffset { get; set; } = new( 6f, 0f, 0f );
	[Property, Group( "Muzzle" )] public Vector3 FallbackWorldOffset { get; set; } = new( 30f, 8f, 54f );

	TimeUntil NextAttackTime { get; set; }
	TimeUntil HostNextAttackTime { get; set; }
	PlayerCombat Combat { get; set; }
	PlayerController Controller { get; set; }
	ValtryekLightningVisual LightningVisual { get; set; }

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Controller = GetComponent<PlayerController>();
		LightningVisual = GetComponent<ValtryekLightningVisual>();
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
	}

	public void TryAttack()
	{
		if ( NextAttackTime > 0f )
			return;

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( Cooldown ) : Cooldown;
		NextAttackTime = cooldown;

		var origin = GetMuzzlePosition();
		var aimPoint = GetAimPoint( origin );
		PlayAttackVisual();
		RequestFireLightning( origin, aimPoint );
	}

	[Rpc.Broadcast]
	void PlayAttackVisual()
	{
		LightningVisual ??= GetComponent<ValtryekLightningVisual>();
		LightningVisual?.PlayAttackVisual();
	}

	[Rpc.Host]
	void RequestFireLightning( Vector3 origin, Vector3 aimPoint )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextAttackTime > 0f )
			return;

		HostNextAttackTime = Combat.IsValid() ? Combat.ScaleCooldown( Cooldown ) : Cooldown;

		var direction = (aimPoint - origin).Normal;
		if ( direction.Length <= 0.01f )
			direction = WorldRotation.Forward.Normal;

		var traceEnd = origin + direction * Range;
		var trace = Scene.Trace
			.Ray( origin, traceEnd )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "deadplayer" )
			.Run();

		var end = trace.Hit ? trace.HitPosition : traceEnd;
		var mainTarget = GetCombatFromTrace( trace );
		if ( mainTarget.IsValid() && !mainTarget.IsDead )
		{
			var amount = ScaleLightningDamage( Damage );
			var impulse = direction * RagdollImpulse + Vector3.Up * RagdollUpwardImpulse;
			mainTarget.ApplyDamage( new DamageEvent( GameObject, amount, DamageType.Magic, trace.HitPosition, impulse ) );
		}

		var branchStarts = new List<Vector3>();
		var branchEnds = new List<Vector3>();
		ApplyBranches( origin, end, mainTarget, branchStarts, branchEnds );

		BroadcastLightningVisual( origin, end, branchStarts.ToArray(), branchEnds.ToArray(), Game.Random.Next( 0, int.MaxValue ) );
	}

	[Rpc.Broadcast]
	void BroadcastLightningVisual( Vector3 start, Vector3 end, Vector3[] branchStarts, Vector3[] branchEnds, int seed )
	{
		var visualObject = new GameObject( true, "Valtryek Lightning Visual" );
		visualObject.NetworkMode = NetworkMode.Never;
		visualObject.WorldPosition = start;
		visualObject.Enabled = true;

		var visual = visualObject.Components.Create<LightningBoltVisual>();
		visual.Start = start;
		visual.End = end;
		visual.SourceObject = GameObject;
		visual.SourceBoneNames = StartBoneNames;
		visual.SourceBoneLocalOffset = BoneLocalOffset;
		visual.SourceFallbackWorldOffset = FallbackWorldOffset;
		visual.Seed = seed;
		visual.PointCount = VisualPointCount;
		visual.Chaos = VisualChaos;
		visual.Lifetime = VisualLifetime;
		visual.BranchStarts = new List<Vector3>( branchStarts ?? Array.Empty<Vector3>() );
		visual.BranchEnds = new List<Vector3>( branchEnds ?? Array.Empty<Vector3>() );
	}

	void ApplyBranches( Vector3 start, Vector3 end, PlayerCombat mainTarget, List<Vector3> branchStarts, List<Vector3> branchEnds )
	{
		var candidates = new List<(PlayerCombat Target, Vector3 BranchStart, float Distance)>();

		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !IsValidBranchTarget( target, mainTarget ) )
				continue;

			var targetPoint = target.WorldPosition + Vector3.Up * 42f;
			var closest = GetClosestPointOnSegment( start, end, targetPoint );
			var distance = (targetPoint - closest).Length;
			if ( distance > BranchRadius )
				continue;

			candidates.Add( (target, closest, distance) );
		}

		candidates.Sort( ( a, b ) => a.Distance.CompareTo( b.Distance ) );
		var count = Math.Min( MaxBranchTargets, candidates.Count );
		for ( var i = 0; i < count; i++ )
		{
			var candidate = candidates[i];
			var hitPosition = candidate.Target.WorldPosition + Vector3.Up * 42f;
			var direction = (hitPosition - candidate.BranchStart).Normal;
			if ( direction.Length <= 0.01f )
				direction = (end - start).Normal;

			var impulse = direction * (RagdollImpulse * 0.45f) + Vector3.Up * (RagdollUpwardImpulse * 0.4f);
			candidate.Target.ApplyDamage( new DamageEvent( GameObject, ScaleLightningDamage( BranchDamage ), DamageType.Magic, hitPosition, impulse ) );
			branchStarts.Add( candidate.BranchStart );
			branchEnds.Add( hitPosition );
		}
	}

	bool IsValidBranchTarget( PlayerCombat target, PlayerCombat mainTarget )
	{
		if ( !target.IsValid() || target.IsDead )
			return false;

		if ( target == mainTarget )
			return false;

		if ( target.GameObject == GameObject )
			return false;

		return true;
	}

	float ScaleLightningDamage( float baseDamage )
	{
		return Combat.IsValid() ? Combat.ScaleDamage( baseDamage, DamageType.Magic ) : baseDamage;
	}

	Vector3 GetMuzzlePosition()
	{
		var startBone = GetStartBoneObject();
		if ( startBone.IsValid() )
			return startBone.WorldPosition + startBone.WorldRotation * BoneLocalOffset;

		return WorldPosition
			+ WorldRotation.Forward * FallbackWorldOffset.x
			+ WorldRotation.Right * FallbackWorldOffset.y
			+ Vector3.Up * FallbackWorldOffset.z;
	}

	GameObject GetStartBoneObject()
	{
		BodyRenderer ??= GameObject.Components.Get<SkinnedModelRenderer>( FindMode.Enabled | FindMode.InDescendants );
		if ( !BodyRenderer.IsValid() || string.IsNullOrWhiteSpace( StartBoneNames ) )
			return null;

		var names = StartBoneNames.Split( new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries );
		foreach ( var rawName in names )
		{
			var boneName = rawName.Trim();
			if ( string.IsNullOrWhiteSpace( boneName ) )
				continue;

			var boneObject = BodyRenderer.GetBoneObject( boneName );
			if ( boneObject.IsValid() )
				return boneObject;
		}

		return null;
	}

	Vector3 GetAimPoint( Vector3 origin )
	{
		var camera = GetMainCamera();
		if ( camera.IsValid() )
		{
			var start = camera.WorldPosition;
			var end = start + camera.WorldRotation.Forward * Range;
			var trace = Scene.Trace
				.Ray( start, end )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "trigger", "deadplayer" )
				.Run();

			return trace.Hit ? trace.HitPosition : end;
		}

		if ( Controller.IsValid() )
			return origin + Controller.EyeAngles.Forward.Normal * Range;

		return origin + WorldRotation.Forward.Normal * Range;
	}

	PlayerCombat GetCombatFromTrace( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return null;

		var hitObject = trace.Collider?.GameObject ?? trace.GameObject;
		if ( !hitObject.IsValid() )
			return null;

		return hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
	}

	CameraComponent GetMainCamera()
	{
		foreach ( var camera in Scene.GetAllComponents<CameraComponent>() )
		{
			if ( camera.IsValid() && camera.IsMainCamera )
				return camera;
		}

		return null;
	}

	static Vector3 GetClosestPointOnSegment( Vector3 start, Vector3 end, Vector3 point )
	{
		var segment = end - start;
		var lengthSquared = Vector3.Dot( segment, segment );
		if ( lengthSquared <= 0.01f )
			return start;

		var t = Vector3.Dot( point - start, segment ) / lengthSquared;
		t = t.Clamp( 0f, 1f );
		return start + segment * t;
	}
}
