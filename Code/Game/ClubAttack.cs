using System;

namespace Sandbox;

[Title( "Club Attack" )]
[Category( "Battle Royale" )]
public sealed class ClubAttack : Component
{
	const int AttackGizmoSegments = 16;

	[Property] public float Damage { get; set; } = 35f;
	[Property] public float Range { get; set; } = 130f;
	[Property] public float ArcDegrees { get; set; } = 80f;
	[Property] public float Cooldown { get; set; } = 0.9f;
	[Property] public bool ShowAttackGizmo { get; set; } = true;
	[Property] public bool ShowAttackGizmoOnSwing { get; set; } = true;
	[Property] public float AttackGizmoDuration { get; set; } = 0.22f;
	[Property] public float AttackGizmoHeight { get; set; } = 8f;
	[Property] public float RagdollImpulse { get; set; } = 520f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 120f;

	TimeUntil NextAttackTime { get; set; }

	PlayerCombat Combat { get; set; }
	PlayerController Controller { get; set; }
	ClubWeaponIk WeaponIk { get; set; }

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Controller = GetComponent<PlayerController>();
		WeaponIk = GetComponent<ClubWeaponIk>();
	}

	public void TryAttack()
	{
		if ( NextAttackTime > 0f )
			return;

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( Cooldown ) : Cooldown;
		NextAttackTime = cooldown;
		RequestSwingClub( WorldPosition, GetAttackForward() );
	}

	[Rpc.Host]
	void RequestSwingClub( Vector3 origin, Vector3 forward )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		PlaySwingVisual( origin, forward );

		var halfArcCos = MathF.Cos( ArcDegrees * 0.5f * MathF.PI / 180f );
		var amount = Combat.IsValid() ? Combat.ScaleDamage( Damage, DamageType.Physical ) : Damage;

		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target == Combat || target.IsDead )
				continue;

			var toTarget = target.WorldPosition - origin;
			var flatDistance = toTarget.WithZ( 0f ).Length;
			if ( flatDistance > Range )
				continue;

			var flatDirection = toTarget.WithZ( 0f ).Normal;
			var facing = forward.WithZ( 0f ).Normal.Dot( flatDirection );
			if ( facing < halfArcCos )
				continue;

			if ( IsBlockedByWall( origin + Vector3.Up * 48f, target.WorldPosition + Vector3.Up * 48f, target.GameObject ) )
				continue;

			var hitPosition = target.WorldPosition + Vector3.Up * 42f;
			var impulse = forward.WithZ( 0f ).Normal * RagdollImpulse + Vector3.Up * RagdollUpwardImpulse;
			target.ApplyDamage( new DamageEvent( GameObject, amount, DamageType.Physical, hitPosition, impulse ) );
		}
	}

	[Rpc.Broadcast]
	void PlaySwingVisual( Vector3 origin, Vector3 forward )
	{
		WeaponIk ??= GetComponent<ClubWeaponIk>();
		WeaponIk?.PlaySwingVisual();
		DrawAttackDebugOverlay( origin, forward );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !ShowAttackGizmo || !Gizmo.IsSelected )
			return;

		DrawAttackGizmoLines( WorldPosition, GetAttackForward() );
	}

	void DrawAttackGizmoLines( Vector3 origin, Vector3 forward )
	{
		var start = GetGizmoOrigin( origin );
		var flatForward = GetFlatDirection( forward );
		var halfArc = ArcDegrees * 0.5f;
		var previous = GetArcPoint( start, flatForward, -halfArc );

		Gizmo.Draw.IgnoreDepth = true;
		Gizmo.Draw.LineThickness = 2f;
		Gizmo.Draw.Color = Color.Red.WithAlpha( 0.85f );
		Gizmo.Draw.Line( start, previous );

		for ( var i = 1; i <= AttackGizmoSegments; i++ )
		{
			var t = i / (float)AttackGizmoSegments;
			var yaw = -halfArc + ArcDegrees * t;
			var current = GetArcPoint( start, flatForward, yaw );
			Gizmo.Draw.Line( previous, current );
			previous = current;
		}

		Gizmo.Draw.Line( start, previous );

		Gizmo.Draw.Color = Color.Yellow.WithAlpha( 0.9f );
		Gizmo.Draw.Line( start, start + flatForward * Range );
	}

	void DrawAttackDebugOverlay( Vector3 origin, Vector3 forward )
	{
		if ( !ShowAttackGizmoOnSwing || AttackGizmoDuration <= 0f )
			return;

		var start = GetGizmoOrigin( origin );
		var flatForward = GetFlatDirection( forward );
		var halfArc = ArcDegrees * 0.5f;
		var previous = GetArcPoint( start, flatForward, -halfArc );
		var edgeColor = Color.Red.WithAlpha( 0.9f );
		var forwardColor = Color.Yellow.WithAlpha( 0.95f );

		DrawDebugLine( start, previous, edgeColor );

		for ( var i = 1; i <= AttackGizmoSegments; i++ )
		{
			var t = i / (float)AttackGizmoSegments;
			var yaw = -halfArc + ArcDegrees * t;
			var current = GetArcPoint( start, flatForward, yaw );
			DrawDebugLine( previous, current, edgeColor );
			previous = current;
		}

		DrawDebugLine( start, previous, edgeColor );
		DrawDebugLine( start, start + flatForward * Range, forwardColor );
	}

	void DrawDebugLine( Vector3 start, Vector3 end, Color color )
	{
		DebugOverlay.Line( start, end, color, AttackGizmoDuration, default( Transform ), false );
	}

	Vector3 GetGizmoOrigin( Vector3 origin )
	{
		return origin + Vector3.Up * AttackGizmoHeight;
	}

	Vector3 GetArcPoint( Vector3 origin, Vector3 flatForward, float yaw )
	{
		var direction = Rotation.FromYaw( yaw ) * flatForward;
		return origin + direction.Normal * Range;
	}

	static Vector3 GetFlatDirection( Vector3 direction )
	{
		var flat = direction.WithZ( 0f );
		return flat.Length > 0.01f ? flat.Normal : Vector3.Forward;
	}

	Vector3 GetAttackForward()
	{
		if ( Controller.IsValid() )
			return GetFlatDirection( Controller.EyeAngles.Forward );

		return GetFlatDirection( WorldRotation.Forward );
	}

	bool IsBlockedByWall( Vector3 origin, Vector3 target, GameObject targetObject )
	{
		var trace = Scene.Trace
			.Ray( origin, target )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "deadplayer" )
			.Run();

		if ( !trace.Hit )
			return false;

		var hitObject = trace.Collider?.GameObject ?? trace.GameObject;
		if ( !hitObject.IsValid() )
			return false;

		return !hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors ).IsValid();
	}
}
