using System.Collections.Generic;

namespace Sandbox;

[Title( "Mycell Attack" )]
[Category( "Battle Royale" )]
public sealed class MycellAttack : Component
{
	[Property] public float Damage { get; set; } = 36f;
	[Property] public float Cooldown { get; set; } = 0.7f;
	[Property] public float Range { get; set; } = 1180f;
	[Property] public float MushroomSpeed { get; set; } = 1020f;
	[Property] public float MushroomUpwardVelocity { get; set; } = 120f;
	[Property] public float MushroomGravity { get; set; } = 780f;
	[Property] public float MushroomMaxLifetime { get; set; } = 6f;
	[Property] public float MushroomHitRadius { get; set; } = 18f;
	[Property] public Model MushroomModel { get; set; }
	[Property] public Color MushroomTint { get; set; } = new( 0.54f, 1f, 0.22f, 1f );
	[Property] public float RagdollImpulse { get; set; } = 110f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 35f;
	[Property, Group( "Poison Cloud" )] public float CloudRadius { get; set; } = 86f;
	[Property, Group( "Poison Cloud" )] public float CloudDuration { get; set; } = 3.2f;
	[Property, Group( "Poison Cloud" )] public float CloudTickInterval { get; set; } = 0.5f;
	[Property, Group( "Poison Cloud" )] public float CloudTickDamage { get; set; } = 12f;
	[Property, Group( "Poison Cloud" )] public float CloudMoveSpeedMultiplier { get; set; } = 0.92f;
	[Property, Group( "Poison Cloud" )] public float CloudSlowDuration { get; set; } = 0.65f;
	[Property, Group( "Poison Cloud" )] public float CloudOwnerMoveSpeedMultiplier { get; set; } = 1.08f;
	[Property, Group( "Poison Cloud" )] public float CloudOwnerBuffDuration { get; set; } = 0.65f;
	[Property, Group( "Poison Cloud" )] public string CloudFogPrefabPath { get; set; } = "prefabs/shroomfog.prefab";
	[Property, Group( "Mushroom Step" )] public float MushroomStepCooldown { get; set; } = 9f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepTrapLifetime { get; set; } = 10f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepMyceliumTrapBonusLifetime { get; set; } = 1f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepTriggerRadius { get; set; } = 96f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepExplosionDamage { get; set; } = 35f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepSlowMultiplier { get; set; } = 0.75f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepSlowDuration { get; set; } = 2f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepMyceliumDuration { get; set; } = 6f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepMyceliumRadius { get; set; } = 105f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepProjectileSpeed { get; set; } = 560f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepProjectileUpwardVelocity { get; set; } = 0f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepProjectileGravity { get; set; } = 780f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepProjectileLifetime { get; set; } = 3f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepProjectileForwardDistance { get; set; } = 240f;
	[Property, Group( "Mushroom Step" )] public float MushroomStepProjectileHitRadius { get; set; } = 18f;
	[Property, Group( "Rotten Heal" )] public float RottenHealCooldown { get; set; } = 14f;
	[Property, Group( "Rotten Heal" )] public float RottenHealRadius { get; set; } = 800f;
	[Property, Group( "Rotten Heal" )] public int RottenHealMaxClouds { get; set; } = 3;
	[Property, Group( "Rotten Heal" )] public float RottenHealAmount { get; set; } = 22f;
	[Property, Group( "Rotten Heal" )] public float RottenHealEmpoweredAmount { get; set; } = 32f;
	[Property, Group( "Rotten Heal" )] public float RottenHealMoveSpeedMultiplier { get; set; } = 1.1f;
	[Property, Group( "Rotten Heal" )] public float RottenHealMoveSpeedDuration { get; set; } = 2f;
	[Property, Group( "Spore Pit" )] public float SporePitCooldown { get; set; } = 45f;
	[Property, Group( "Spore Pit" )] public float SporePitRadius { get; set; } = 900f;
	[Property, Group( "Spore Pit" )] public float SporePitDuration { get; set; } = 18f;
	[Property, Group( "Spore Pit" )] public float SporePitExitGraceSeconds { get; set; } = 0.15f;
	[Property, Group( "Spore Pit" )] public float SporePitTickDamage { get; set; } = 10f;
	[Property, Group( "Spore Pit" )] public float SporePitEnemySlowMultiplier { get; set; } = 0.85f;
	[Property, Group( "Spore Pit" )] public float SporePitOwnerMoveSpeedMultiplier { get; set; } = 1.1f;
	[Property, Group( "Spore Pit" )] public float SporePitVolumetricFogStrength { get; set; } = 0.42f;
	[Property, Group( "Spore Pit" )] public float SporePitVolumetricFogFalloffExponent { get; set; } = 2.2f;
	[Property, Group( "Spore Pit" )] public Color SporePitColor { get; set; } = new( 0.24f, 1f, 0.02f, 0.95f );

	TimeUntil NextAttackTime { get; set; }
	TimeUntil HostNextAttackTime { get; set; }
	TimeUntil NextMushroomStepTime { get; set; }
	TimeUntil HostNextMushroomStepTime { get; set; }
	TimeUntil NextRottenHealTime { get; set; }
	TimeUntil HostNextRottenHealTime { get; set; }
	TimeUntil NextSporePitTime { get; set; }
	TimeUntil HostNextSporePitTime { get; set; }
	MycellSporePit ActiveSporePit { get; set; }

	PlayerCombat Combat { get; set; }
	PlayerController Controller { get; set; }
	MycellThrowVisual ThrowVisual { get; set; }

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Controller = GetComponent<PlayerController>();
		ThrowVisual = GetComponent<MycellThrowVisual>();
		MushroomModel ??= Model.Load( "models/fungus2.vmdl" );

	}

	public void TryAttack()
	{
		if ( NextAttackTime > 0f )
			return;

		var cooldown = GetScaledCooldown();
		NextAttackTime = cooldown;

		var origin = WorldPosition + Vector3.Up * 54f + WorldRotation.Forward * 30f + WorldRotation.Right * 8f;
		var aimPoint = GetAimPoint( origin );
		PlayThrowVisual( cooldown );
		RequestThrowMushroom( origin, aimPoint );
	}

	public void TryMushroomStep()
	{
		if ( NextMushroomStepTime > 0f )
			return;

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( MushroomStepCooldown ) : MushroomStepCooldown;
		NextMushroomStepTime = cooldown;
		RequestMushroomStep();
	}

	public void TryRottenHeal()
	{
		if ( NextRottenHealTime > 0f )
			return;

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( RottenHealCooldown ) : RottenHealCooldown;
		NextRottenHealTime = cooldown;
		RequestRottenHeal();
	}

	public void TrySporePit()
	{
		if ( ActiveSporePit.IsValid() )
		{
			RequestDisperseSporePit();
			return;
		}

		if ( NextSporePitTime > 0f )
			return;

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( SporePitCooldown ) : SporePitCooldown;
		NextSporePitTime = cooldown;
		RequestSporePit();
	}

	[Rpc.Broadcast]
	void PlayThrowVisual( float cooldown )
	{
		ThrowVisual ??= GetComponent<MycellThrowVisual>();
		ThrowVisual?.PlayThrowVisual( cooldown );
	}

	[Rpc.Broadcast]
	void PlayUseVisual()
	{
		ThrowVisual ??= GetComponent<MycellThrowVisual>();
		ThrowVisual?.PlayUseVisual();
	}

	[Rpc.Host]
	void RequestThrowMushroom( Vector3 origin, Vector3 aimPoint )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextAttackTime > 0f )
			return;

		HostNextAttackTime = GetScaledCooldown();

		var direction = (aimPoint - origin).Normal;
		if ( direction.Length <= 0.01f )
			direction = WorldRotation.Forward.Normal;

		SpawnMushroomProjectile( origin, direction );
	}

	[Rpc.Host]
	void RequestMushroomStep()
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextMushroomStepTime > 0f )
			return;

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( MushroomStepCooldown ) : MushroomStepCooldown;
		HostNextMushroomStepTime = cooldown;

		var origin = WorldPosition;
		var empowered = IsInsideOwnMycelium( origin );
		var trapLifetime = MushroomStepTrapLifetime + (empowered ? MushroomStepMyceliumTrapBonusLifetime : 0f);
		SpawnMushroomStepSeed( GetMushroomStepSeedOrigin( origin ), GetMushroomStepSeedDirection(), trapLifetime );
	}

	[Rpc.Host]
	void RequestRottenHeal()
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( HostNextRottenHealTime > 0f )
			return;

		var clouds = FindConsumablePoisonClouds();
		if ( clouds.Count <= 0 )
		{
			ResetRottenHealCooldown();
			return;
		}

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( RottenHealCooldown ) : RottenHealCooldown;
		HostNextRottenHealTime = cooldown;
		PlayUseVisual();

		var healAmount = 0f;
		foreach ( var cloud in clouds )
		{
			if ( !cloud.IsValid() )
				continue;

			healAmount += cloud.HasDamagedEnemy ? RottenHealEmpoweredAmount : RottenHealAmount;
			cloud.Consume();
		}

		if ( Combat.IsValid() )
		{
			Combat.Heal( healAmount );
			Combat.ApplyMoveSpeedMultiplierEffect( RottenHealMoveSpeedMultiplier, RottenHealMoveSpeedDuration );
		}
	}

	[Rpc.Host]
	void RequestSporePit()
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( ActiveSporePit.IsValid() )
			return;

		if ( HostNextSporePitTime > 0f )
			return;

		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( SporePitCooldown ) : SporePitCooldown;
		HostNextSporePitTime = cooldown;
		SpawnSporePit( WorldPosition );
		PlayUseVisual();
	}

	[Rpc.Host]
	void RequestDisperseSporePit()
	{
		if ( ActiveSporePit.IsValid() )
			BroadcastDisperseSporePit();
	}

	[Rpc.Broadcast]
	void ResetRottenHealCooldown()
	{
		NextRottenHealTime = 0f;
		HostNextRottenHealTime = 0f;
	}

	[Rpc.Broadcast]
	void SpawnMushroomProjectile( Vector3 origin, Vector3 direction )
	{
		var projectileObject = new GameObject( true, "Mycell Toxic Mushroom" );
		projectileObject.NetworkMode = NetworkMode.Never;
		projectileObject.WorldPosition = origin;
		projectileObject.WorldRotation = Rotation.LookAt( direction.Normal, Vector3.Up );

		if ( MushroomModel.IsValid() )
		{
			var renderer = projectileObject.Components.Create<ModelRenderer>();
			renderer.Model = MushroomModel;
			renderer.Tint = MushroomTint;
		}

		var projectile = projectileObject.Components.Create<MycellMushroomProjectile>();
		projectile.Direction = direction.Normal;
		projectile.MaxDistance = Range;
		projectile.Speed = MushroomSpeed;
		projectile.UpwardVelocity = MushroomUpwardVelocity;
		projectile.Gravity = MushroomGravity;
		projectile.MaxLifetime = MushroomMaxLifetime;
		projectile.HitRadius = MushroomHitRadius;
		projectile.Source = GameObject;
		projectile.OwnerAttack = this;
		projectile.Damage = Damage;
		projectile.RagdollImpulse = RagdollImpulse;
		projectile.RagdollUpwardImpulse = RagdollUpwardImpulse;
		projectile.CloudRadius = CloudRadius;
		projectile.CloudDuration = CloudDuration;
		projectile.CloudTickInterval = CloudTickInterval;
		projectile.CloudTickDamage = CloudTickDamage;
		projectile.CloudMoveSpeedMultiplier = CloudMoveSpeedMultiplier;
		projectile.CloudSlowDuration = CloudSlowDuration;
		projectile.CloudOwnerMoveSpeedMultiplier = CloudOwnerMoveSpeedMultiplier;
		projectile.CloudOwnerBuffDuration = CloudOwnerBuffDuration;
		projectile.CloudFogPrefabPath = CloudFogPrefabPath;
		projectile.DebugColor = MushroomTint;
	}

	public float ScaleMycellDamage( float baseDamage )
	{
		return Combat.IsValid() ? Combat.ScaleDamage( baseDamage, DamageType.Magic ) : baseDamage;
	}

	public float MushroomStepCooldownRemaining => NextMushroomStepTime > 0f ? NextMushroomStepTime : 0f;
	public float MushroomStepCooldownPercent => MushroomStepCooldown <= 0f ? 0f : (MushroomStepCooldownRemaining / MushroomStepCooldown * 100f).Clamp( 0f, 100f );
	public float RottenHealCooldownRemaining => NextRottenHealTime > 0f ? NextRottenHealTime : 0f;
	public float RottenHealCooldownPercent => RottenHealCooldown <= 0f ? 0f : (RottenHealCooldownRemaining / RottenHealCooldown * 100f).Clamp( 0f, 100f );
	public float SporePitCooldownRemaining => NextSporePitTime > 0f ? NextSporePitTime : 0f;
	public float SporePitCooldownPercent => SporePitCooldown <= 0f ? 0f : (SporePitCooldownRemaining / SporePitCooldown * 100f).Clamp( 0f, 100f );
	public bool IsSporePitActive => ActiveSporePit.IsValid();

	float GetScaledCooldown()
	{
		return Combat.IsValid() ? Combat.ScaleCooldown( Cooldown ) : Cooldown;
	}

	bool IsInsideOwnMycelium( Vector3 position )
	{
		foreach ( var patch in Scene.GetAllComponents<MycellMyceliumPatch>() )
		{
			if ( patch.IsValid() && patch.Source == GameObject && patch.Contains( position ) )
				return true;
		}

		return false;
	}

	List<MycellPoisonCloud> FindConsumablePoisonClouds()
	{
		var clouds = new List<MycellPoisonCloud>();
		foreach ( var cloud in Scene.GetAllComponents<MycellPoisonCloud>() )
		{
			if ( !cloud.IsValid() || cloud.Source != GameObject )
				continue;

			if ( (cloud.WorldPosition - WorldPosition).Length > RottenHealRadius )
				continue;

			clouds.Add( cloud );
		}

		clouds.Sort( ( a, b ) => (a.WorldPosition - WorldPosition).Length.CompareTo( (b.WorldPosition - WorldPosition).Length ) );
		if ( clouds.Count > RottenHealMaxClouds )
			clouds.RemoveRange( RottenHealMaxClouds, clouds.Count - RottenHealMaxClouds );

		return clouds;
	}

	void SpawnMushroomStepSeed( Vector3 origin, Vector3 direction, float trapLifetime )
	{
		var seedObject = new GameObject( true, "Mycell Mushroom Step Seed" );
		seedObject.NetworkMode = NetworkMode.Never;
		seedObject.WorldPosition = origin;
		seedObject.WorldRotation = Rotation.LookAt( direction, Vector3.Up );

		if ( MushroomModel.IsValid() )
		{
			var renderer = seedObject.Components.Create<ModelRenderer>();
			renderer.Model = MushroomModel;
			renderer.Tint = MushroomTint;
		}

		var seed = seedObject.Components.Create<MycellMushroomStepSeed>();
		seed.Source = GameObject;
		seed.OwnerAttack = this;
		seed.Direction = direction;
		seed.Speed = MushroomStepProjectileSpeed;
		seed.UpwardVelocity = MushroomStepProjectileUpwardVelocity;
		seed.Gravity = MushroomStepProjectileGravity;
		seed.MaxLifetime = MushroomStepProjectileLifetime;
		seed.HitRadius = MushroomStepProjectileHitRadius;
		seed.TrapLifetime = trapLifetime;
		seed.TrapRadius = MushroomStepTriggerRadius;
		seed.TrapDamage = MushroomStepExplosionDamage;
		seed.TrapSlowMultiplier = MushroomStepSlowMultiplier;
		seed.TrapSlowDuration = MushroomStepSlowDuration;
		seed.CloudRadius = CloudRadius;
		seed.CloudDuration = CloudDuration;
		seed.CloudTickInterval = CloudTickInterval;
		seed.CloudTickDamage = CloudTickDamage;
		seed.CloudMoveSpeedMultiplier = CloudMoveSpeedMultiplier;
		seed.CloudSlowDuration = CloudSlowDuration;
		seed.CloudOwnerMoveSpeedMultiplier = CloudOwnerMoveSpeedMultiplier;
		seed.CloudOwnerBuffDuration = CloudOwnerBuffDuration;
		seed.CloudFogPrefabPath = CloudFogPrefabPath;
		seed.MyceliumDuration = MushroomStepMyceliumDuration;
		seed.MyceliumRadius = MushroomStepMyceliumRadius;
		seed.MushroomModel = MushroomModel;
		seed.MushroomTint = MushroomTint;
	}

	[Rpc.Broadcast]
	void SpawnSporePit( Vector3 position )
	{
		if ( ActiveSporePit.IsValid() )
			ActiveSporePit.DestroyLocal();

		var pitObject = new GameObject( true, "Mycell Spore Pit" );
		pitObject.NetworkMode = NetworkMode.Never;
		pitObject.WorldPosition = position;

		var pit = pitObject.Components.Create<MycellSporePit>();
		pit.Source = GameObject;
		pit.OwnerAttack = this;
		pit.Radius = SporePitRadius;
		pit.Duration = SporePitDuration;
		pit.ExitGraceSeconds = SporePitExitGraceSeconds;
		pit.TickDamage = SporePitTickDamage;
		pit.EnemySlowMultiplier = SporePitEnemySlowMultiplier;
		pit.OwnerMoveSpeedMultiplier = SporePitOwnerMoveSpeedMultiplier;
		pit.VolumetricFogStrength = SporePitVolumetricFogStrength;
		pit.VolumetricFogFalloffExponent = SporePitVolumetricFogFalloffExponent;
		pit.FogPrefabPath = CloudFogPrefabPath;
		pit.PitColor = SporePitColor;
		ActiveSporePit = pit;
	}

	public void BroadcastDisperseSporePit()
	{
		if ( !Networking.IsHost )
			return;

		DisperseSporePitLocal();
	}

	[Rpc.Broadcast]
	void DisperseSporePitLocal()
	{
		if ( !ActiveSporePit.IsValid() )
			return;

		var pit = ActiveSporePit;
		ActiveSporePit = null;
		pit.DestroyLocal();
	}

	Vector3 GetMushroomStepSeedOrigin( Vector3 origin )
	{
		return origin + Vector3.Up * 40f + WorldRotation.Forward * 28f;
	}

	Vector3 GetMushroomStepSeedDirection()
	{
		var camera = GetMainCamera();
		var direction = camera.IsValid() ? camera.WorldRotation.Forward : WorldRotation.Forward;
		if ( direction.Length <= 0.01f )
			direction = WorldRotation.Forward;

		if ( direction.Length <= 0.01f )
			direction = Vector3.Forward;

		var target = GetMushroomStepSeedOrigin( WorldPosition ) + direction.Normal * MushroomStepProjectileForwardDistance;
		return (target - GetMushroomStepSeedOrigin( WorldPosition )).Normal;
	}

	void SpawnMyceliumPatch( Vector3 position, float duration )
	{
		var patchObject = new GameObject( true, "Mycell Mycelium Patch" );
		patchObject.NetworkMode = NetworkMode.Never;
		patchObject.WorldPosition = position;

		var patch = patchObject.Components.Create<MycellMyceliumPatch>();
		patch.Source = GameObject;
		patch.Radius = MushroomStepMyceliumRadius;
		patch.Duration = duration;
		patch.DebugColor = MushroomTint;
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

	CameraComponent GetMainCamera()
	{
		foreach ( var camera in Scene.GetAllComponents<CameraComponent>() )
		{
			if ( camera.IsValid() && camera.IsMainCamera )
				return camera;
		}

		return null;
	}
}
