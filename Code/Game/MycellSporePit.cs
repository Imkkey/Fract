using System;
using System.Collections.Generic;

namespace Sandbox;

[Title( "Mycell Spore Pit" )]
[Category( "Battle Royale" )]
public sealed class MycellSporePit : Component
{
	public GameObject Source { get; set; }
	public MycellAttack OwnerAttack { get; set; }
	public float Radius { get; set; } = 900f;
	public float Duration { get; set; } = 18f;
	public float ExitGraceSeconds { get; set; } = 0.15f;
	public float TickInterval { get; set; } = 1f;
	public float TickDamage { get; set; } = 10f;
	public float EnemySlowMultiplier { get; set; } = 0.85f;
	public float EnemySlowDuration { get; set; } = 1.2f;
	public float OwnerMoveSpeedMultiplier { get; set; } = 1.1f;
	public float OwnerBuffDuration { get; set; } = 1.2f;
	public float InteriorFogScale { get; set; } = 3.2f;
	public float InteriorFogEmitterRadius { get; set; } = 150f;
	public float InteriorFogEmitterRate { get; set; } = 35f;
	public float VolumetricFogStrength { get; set; } = 0.42f;
	public float VolumetricFogFalloffExponent { get; set; } = 2.2f;
	public float DomeCenterHeight { get; set; } = 160f;
	public float DomeVerticalScale { get; set; } = 0.62f;
	public float SphereModelBaseRadius { get; set; } = 50f;
	public string FogPrefabPath { get; set; } = "prefabs/shroomfog.prefab";
	public string DomeMaterialPath { get; set; } = "materials/mycell_spore_pit_dome.vmat";
	public Color PitColor { get; set; } = new( 0.24f, 1f, 0.02f, 0.95f );

	TimeUntil LifeTime { get; set; }
	TimeUntil NextTickTime { get; set; }
	float OwnerOutsideSeconds { get; set; }
	GameObject VisualRoot { get; set; }
	GameObject FogSphere { get; set; }
	GameObject InnerFogSphere { get; set; }
	GameObject VolumeObject { get; set; }
	List<GameObject> InteriorFogObjects { get; } = new();
	Vector3 FogSphereScale { get; set; } = Vector3.One;
	Vector3 InnerFogSphereScale { get; set; } = Vector3.One;
	float DomeHorizontalRadius { get; set; }
	float DomeVerticalRadius { get; set; }
	bool IsEnding { get; set; }
	float EffectRadius => Radius;

	protected override void OnStart()
	{
		LifeTime = Duration;
		OwnerOutsideSeconds = 0f;
		NextTickTime = 0f;
		SpawnVisuals();
	}

	protected override void OnUpdate()
	{
		if ( !Source.IsValid() || IsEnding || LifeTime <= 0f )
		{
			EndPit();
			return;
		}

		if ( Networking.IsHost )
		{
			UpdateOwnerIntegrity();
			if ( NextTickTime <= 0f )
			{
				NextTickTime = TickInterval;
				ApplyPitEffects();
			}
		}

		UpdateVisuals();
	}

	public void Disperse()
	{
		if ( Networking.IsHost && OwnerAttack.IsValid() )
		{
			OwnerAttack.BroadcastDisperseSporePit();
			return;
		}

		DestroyLocal();
	}

	public void DestroyLocal()
	{
		if ( IsEnding )
			return;

		IsEnding = true;
		GameObject.Destroy();
	}

	bool IsInsidePit( Vector3 position )
	{
		return (position - WorldPosition).WithZ( 0f ).Length <= EffectRadius;
	}

	void UpdateOwnerIntegrity()
	{
		if ( !IsInsidePit( Source.WorldPosition ) )
		{
			OwnerOutsideSeconds += Time.Delta;
			if ( OwnerOutsideSeconds >= ExitGraceSeconds )
				EndPit();

			return;
		}

		OwnerOutsideSeconds = 0f;
	}

	void ApplyPitEffects()
	{
		foreach ( var target in Scene.GetAllComponents<PlayerCombat>() )
		{
			if ( !target.IsValid() || target.IsDead )
				continue;

			if ( !IsInsidePit( target.WorldPosition ) )
				continue;

			if ( target.GameObject == Source )
			{
				target.ApplyMoveSpeedMultiplierEffect( OwnerMoveSpeedMultiplier, OwnerBuffDuration );
				continue;
			}

			var damage = OwnerAttack.IsValid() ? OwnerAttack.ScaleMycellDamage( TickDamage ) : TickDamage;
			target.ApplyDamage( new DamageEvent( Source, damage, DamageType.Magic, target.WorldPosition + Vector3.Up * 36f ) );
			target.ApplyMoveSlow( EnemySlowMultiplier, EnemySlowDuration );
		}
	}

	void SpawnVisuals()
	{
		VisualRoot = new GameObject( GameObject, false, "Spore Pit Visuals" );
		VisualRoot.NetworkMode = NetworkMode.Never;
		VisualRoot.LocalPosition = Vector3.Zero;
		VisualRoot.Enabled = true;

		SpawnFogSphere();
		SpawnInteriorFog();
		SpawnToxicPitVolume();
	}

	void UpdateVisuals()
	{
		var pulse = ((float)Math.Sin( Time.Now * 1.4f ) + 1f) * 0.5f;
		var breathe = 0.94f + pulse * 0.1f;

		if ( FogSphere.IsValid() )
		{
			FogSphere.LocalScale = FogSphereScale;
			FogSphere.LocalRotation = Rotation.FromYaw( Time.Now * 2.5f );
		}

		if ( InnerFogSphere.IsValid() )
		{
			InnerFogSphere.LocalScale = InnerFogSphereScale * (1.01f - pulse * 0.045f);
			InnerFogSphere.LocalRotation = Rotation.FromYaw( -Time.Now * 3.25f );
		}

		UpdateInteriorFog( pulse );
	}

	void SpawnFogSphere()
	{
		DomeHorizontalRadius = GetDomeHorizontalRadius();
		DomeVerticalRadius = Radius * DomeVerticalScale;
		var horizontalScale = DomeHorizontalRadius / SphereModelBaseRadius;
		var verticalScale = DomeVerticalRadius / SphereModelBaseRadius;
		var material = Material.Load( DomeMaterialPath );

		FogSphere = CreateFogSphere( "FogSphere", new Vector3( horizontalScale, horizontalScale, verticalScale ), PitColor.WithAlpha( 0.28f ), material );
		FogSphereScale = FogSphere.LocalScale;

		InnerFogSphere = CreateFogSphere( "InnerFogSphere", new Vector3( horizontalScale * 0.94f, horizontalScale * 0.94f, verticalScale * 0.94f ), PitColor.WithAlpha( 0.42f ), material );
		InnerFogSphereScale = InnerFogSphere.LocalScale;
	}

	float GetDomeHorizontalRadius()
	{
		var verticalRadius = Radius * DomeVerticalScale;
		if ( verticalRadius <= DomeCenterHeight + 1f )
			return Radius;

		var centerToGround = DomeCenterHeight / verticalRadius;
		var groundSlice = 1f - (float)(centerToGround * centerToGround);
		if ( groundSlice <= 0.01f )
			return Radius;

		return Radius / (float)Math.Sqrt( groundSlice );
	}

	GameObject CreateFogSphere( string name, Vector3 scale, Color tint, Material material )
	{
		var sphere = new GameObject( VisualRoot, false, name );
		sphere.NetworkMode = NetworkMode.Never;
		sphere.LocalPosition = Vector3.Up * DomeCenterHeight;
		sphere.LocalScale = scale;
		sphere.Enabled = true;

		var renderer = sphere.Components.Create<ModelRenderer>();
		renderer.Model = Model.Sphere;
		renderer.Tint = tint;

		if ( material is not null )
			renderer.Materials.SetOverride( 0, material );

		return sphere;
	}

	void SpawnInteriorFog()
	{
		InteriorFogObjects.Clear();

		if ( string.IsNullOrWhiteSpace( FogPrefabPath ) )
			return;

		var offsets = new[]
		{
			Vector3.Zero,
			new Vector3( 0.22f, 0f, 0f ),
			new Vector3( -0.22f, 0f, 0f ),
			new Vector3( 0f, 0.22f, 0f ),
			new Vector3( 0f, -0.22f, 0f ),
			new Vector3( 0.16f, 0.16f, 0f ),
			new Vector3( -0.16f, 0.16f, 0f ),
			new Vector3( 0.16f, -0.16f, 0f ),
			new Vector3( -0.16f, -0.16f, 0f )
		};

		var scale = Math.Max( 1f, Radius / 900f * InteriorFogScale );
		for ( var i = 0; i < offsets.Length; i++ )
		{
			var fog = GameObject.Clone( FogPrefabPath, WorldTransform, VisualRoot, false, $"Spore Pit Interior Fog {i + 1}" );
			if ( !fog.IsValid() )
				continue;

			fog.NetworkMode = NetworkMode.Never;
			fog.Enabled = true;
			fog.LocalPosition = offsets[i] * Radius + Vector3.Up * 70f;
			fog.LocalRotation = Rotation.FromYaw( i * 41f );
			fog.LocalScale = Vector3.One * scale;
			ConfigureInteriorFogParticles( fog );
			InteriorFogObjects.Add( fog );
		}
	}

	void ConfigureInteriorFogParticles( GameObject fog )
	{
		var emitter = fog.Components.Get<ParticleSphereEmitter>();
		if ( emitter.IsValid() )
		{
			emitter.Radius = InteriorFogEmitterRadius;
			emitter.Rate = InteriorFogEmitterRate;
			emitter.OnEdge = false;
		}

		var particles = fog.Components.Get<ParticleEffect>();
		if ( particles.IsValid() )
		{
			particles.ForceScale = 0.28f;
			particles.Damping = 8f;
			particles.Brightness = 1.25f;
			particles.Tint = PitColor.WithAlpha( 1f );
		}
	}

	void UpdateInteriorFog( float pulse )
	{
		var scale = Math.Max( 1f, Radius / 900f * InteriorFogScale );
		var breathe = 0.96f + pulse * 0.08f;

		for ( var i = 0; i < InteriorFogObjects.Count; i++ )
		{
			var fog = InteriorFogObjects[i];
			if ( !fog.IsValid() )
				continue;

			fog.LocalRotation = Rotation.FromYaw( Time.Now * (1.4f + i * 0.08f) + i * 41f );
			fog.LocalScale = Vector3.One * scale * breathe;
		}
	}

	void SpawnToxicPitVolume()
	{
		VolumeObject = new GameObject( VisualRoot, false, "TriggerSphere" );
		VolumeObject.NetworkMode = NetworkMode.Never;
		VolumeObject.LocalPosition = Vector3.Zero;
		VolumeObject.LocalScale = Vector3.One * (EffectRadius / SphereModelBaseRadius);
		VolumeObject.Enabled = true;

		var pitVolume = VolumeObject.Components.Create<MycellToxicPitVolume>();
		pitVolume.Source = Source;
		pitVolume.Radius = EffectRadius;

		var volumetricFog = VolumeObject.Components.Create<VolumetricFogVolume>();
		volumetricFog.Bounds = new BBox(
			new Vector3( -EffectRadius, -EffectRadius, 0f ),
			new Vector3( EffectRadius, EffectRadius, DomeCenterHeight + DomeVerticalRadius )
		);
		volumetricFog.Strength = VolumetricFogStrength;
		volumetricFog.FalloffExponent = VolumetricFogFalloffExponent;

		var trigger = VolumeObject.Components.Create<SphereCollider>();
		trigger.IsTrigger = true;
	}

	void EndPit()
	{
		if ( IsEnding )
			return;

		if ( Networking.IsHost && OwnerAttack.IsValid() )
		{
			OwnerAttack.BroadcastDisperseSporePit();
			return;
		}

		DestroyLocal();
	}

	protected override void OnDestroy()
	{
		if ( VisualRoot.IsValid() )
			VisualRoot.Destroy();
	}
}
