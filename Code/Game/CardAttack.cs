using System;

namespace Sandbox;

[Title( "Card Attack" )]
[Category( "Battle Royale" )]
public sealed class CardAttack : Component
{
	[Property] public float Damage { get; set; } = 38f;
	[Property] public float Range { get; set; } = 1024f;
	[Property] public float Cooldown { get; set; } = 0.65f;
	[Property] public float SpreadDegrees { get; set; } = 2f;
	[Property] public float VerticalCardSpacing { get; set; } = 2f;
	[Property] public float RagdollImpulse { get; set; } = 180f;
	[Property] public float RagdollUpwardImpulse { get; set; } = 45f;
	[Property] public Model CardModel { get; set; }
	[Property] public float CardVisualSpeed { get; set; } = 1654f;
	[Property] public int MaxPiercedEnemies { get; set; } = 2;
	[Property] public float DamageFalloffPerPierce { get; set; } = 0.35f;
	[Property, Group( "Skill Stub" )] public float SkillStubBaseDamage { get; set; } = 30f;
	[Property, Group( "Shuffle Dash" )] public float ShuffleDashDistance { get; set; } = 180f;
	[Property, Group( "Shuffle Dash" )] public float ShuffleDashCooldown { get; set; } = 6f;
	[Property, Group( "Shuffle Dash" )] public float ShuffleDashBuffDuration { get; set; } = 2f;
	[Property, Group( "Shuffle Dash" )] public float ShuffleDashCardSpeedMultiplier { get; set; } = 1.35f;
	[Property, Group( "Shuffle Dash" )] public float ShuffleDashCardRangeMultiplier { get; set; } = 1.35f;
	[Property, Group( "Marked Deck" )] public float MarkedDeckRevealDuration { get; set; } = 4f;
	[Property, Group( "Marked Deck" )] public float MarkedDeckPassiveInterval { get; set; } = 10f;
	[Property, Group( "Marked Deck" )] public float MarkedDeckPhysicalDamageMultiplier { get; set; } = 1.2f;
	[Property, Group( "Marked Deck" )] public Color MarkedDeckCardTint { get; set; } = new( 1f, 0.78f, 0.12f, 1f );
	[Property, Group( "Loaded Hand" )] public float BloodCardDirectDamageMultiplier { get; set; } = 0.7f;
	[Property, Group( "Loaded Hand" )] public float BloodCardBleedDamage { get; set; } = 42f;
	[Property, Group( "Loaded Hand" )] public float BloodCardBleedDuration { get; set; } = 4f;
	[Property, Group( "Loaded Hand" )] public float BloodCardHealFraction { get; set; } = 0.5f;
	[Property, Group( "Loaded Hand" )] public float AceCardBleedLuckBonusDamage { get; set; } = 70f;

	TimeUntil NextAttackTime { get; set; }
	TimeUntil NextShuffleDashTime { get; set; }
	TimeUntil ShuffleDashBuffTime { get; set; }
	bool MarkedDeckReady { get; set; }
	bool PredictedMarkedDeckReady { get; set; }
	int MarkedDeckCardIndex { get; set; } = -1;
	TimeUntil NextMarkedDeckPassiveTime { get; set; }
	LoadedHandCard PredictedLoadedHandCard { get; set; } = LoadedHandCard.Eye;
	public bool IsLoadedHandPanelOpen { get; private set; }

	PlayerCombat Combat { get; set; }
	PlayerController Controller { get; set; }
	CardThrowVisual ThrowVisual { get; set; }
	int NextAttackId { get; set; }
	Dictionary<int, Dictionary<PlayerCombat, int>> AttackCardHits { get; } = new();

	public float AttackCooldownRemaining => MathF.Max( 0f, NextAttackTime );
	public float AttackCooldownPercent => Cooldown <= 0f ? 0f : (AttackCooldownRemaining / Cooldown * 100f).Clamp( 0f, 100f );
	public float ShuffleDashCooldownRemaining => MathF.Max( 0f, NextShuffleDashTime );
	public float ShuffleDashCooldownPercent => ShuffleDashCooldown <= 0f ? 0f : (ShuffleDashCooldownRemaining / ShuffleDashCooldown * 100f).Clamp( 0f, 100f );
	public bool IsShuffleDashBuffActive => ShuffleDashBuffTime > 0f;
	public bool IsMarkedDeckLoaded => MarkedDeckReady || PredictedMarkedDeckReady;
	public float MarkedDeckPassiveCooldownRemaining => IsMarkedDeckLoaded ? 0f : MathF.Max( 0f, NextMarkedDeckPassiveTime );
	public float MarkedDeckPassiveCooldownPercent => MarkedDeckPassiveInterval <= 0f ? 0f : (MarkedDeckPassiveCooldownRemaining / MarkedDeckPassiveInterval * 100f).Clamp( 0f, 100f );
	[Sync( Flags = SyncFlags.FromHost )] public LoadedHandCard SelectedLoadedHandCard { get; private set; } = LoadedHandCard.Eye;
	public LoadedHandCard DisplayLoadedHandCard => GameObject.Network.IsOwner ? PredictedLoadedHandCard : SelectedLoadedHandCard;

	protected override void OnStart()
	{
		Combat = GetComponent<PlayerCombat>();
		Controller = GetComponent<PlayerController>();
		ThrowVisual = GetComponent<CardThrowVisual>();
		CardModel ??= Model.Load( "models/card.vmdl" );
		NextMarkedDeckPassiveTime = MarkedDeckPassiveInterval;
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost )
			UpdateMarkedDeckPassive();
	}

	public void TryAttack()
	{
		if ( NextAttackTime > 0f )
			return;

		var origin = WorldPosition + Vector3.Up * 52f + WorldRotation.Forward * 32f + WorldRotation.Right * 10f;
		var aimPoint = GetAimPoint( origin );
		var cooldown = Combat.IsValid() ? Combat.ScaleCooldown( Cooldown ) : Cooldown;

		NextAttackTime = cooldown;
		PlayThrowVisual();
		RequestThrowCards( origin, aimPoint );

		if ( IsMarkedDeckLoaded )
		{
			PredictedMarkedDeckReady = false;
			NextMarkedDeckPassiveTime = MarkedDeckPassiveInterval;
		}
	}

	public void TryShuffleDash()
	{
		if ( NextShuffleDashTime > 0f )
			return;

		var direction = GetShuffleDashDirection();
		if ( direction.Length <= 0.01f )
			return;

		NextShuffleDashTime = ShuffleDashCooldown;
		RequestShuffleDash( direction.Normal );
	}

	public void TryCycleLoadedHand()
	{
		if ( !IsLoadedHandPanelOpen )
		{
			IsLoadedHandPanelOpen = true;
			return;
		}

		var nextCard = GetNextLoadedHandCard( DisplayLoadedHandCard );
		PredictedLoadedHandCard = nextCard;
		RequestSetLoadedHandCard( nextCard );
	}

	[Rpc.Broadcast]
	void PlayThrowVisual()
	{
		ThrowVisual ??= GetComponent<CardThrowVisual>();
		ThrowVisual?.PlayThrowVisual();
	}

	[Rpc.Host]
	void RequestThrowCards( Vector3 origin, Vector3 aimPoint )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		var forward = (aimPoint - origin).Normal;
		if ( forward.Length <= 0.01f )
			forward = WorldRotation.Forward.Normal;

		var attackId = NextAttackId++;
		AttackCardHits[attackId] = new Dictionary<PlayerCombat, int>();
		var projectileRange = GetCurrentCardRange();
		var projectileSpeed = GetCurrentCardSpeed();
		var markedCardIndex = ConsumeMarkedDeckCardIndex();

		for ( var i = 0; i < 3; i++ )
		{
			var yaw = (i - 1) * SpreadDegrees;
			var direction = Rotation.FromYaw( yaw ) * forward;
			var cardOrigin = origin + Vector3.Up * ((1 - i) * VerticalCardSpacing);
			SpawnCardProjectile( cardOrigin, direction.Normal, attackId, projectileRange, projectileSpeed, i == markedCardIndex );
		}
	}

	[Rpc.Broadcast]
	void SpawnCardProjectile( Vector3 origin, Vector3 direction, int attackId, float projectileRange, float projectileSpeed, bool isMarkedDeckCard )
	{
		CardModel ??= Model.Load( "models/card.vmdl" );
		if ( !CardModel.IsValid() )
			return;

		var cardObject = new GameObject( true, "Card Projectile Visual" );
		cardObject.NetworkMode = NetworkMode.Never;
		cardObject.WorldPosition = origin;
		cardObject.WorldRotation = Rotation.LookAt( direction.Normal, Vector3.Up );

		var renderer = cardObject.Components.Create<ModelRenderer>();
		renderer.Model = CardModel;
		if ( isMarkedDeckCard )
			renderer.Tint = MarkedDeckCardTint;

		var projectile = cardObject.Components.Create<CardProjectileVisual>();
		projectile.Direction = direction.Normal;
		projectile.MaxDistance = projectileRange;
		projectile.Speed = projectileSpeed;
		projectile.Source = GameObject;
		projectile.OwnerAttack = this;
		projectile.AttackId = attackId;
		projectile.Damage = Damage;
		projectile.DamageTypeAfterPierce = DamageType.Magic;
		projectile.InitialDamageType = DamageType.Physical;
		projectile.RagdollImpulse = RagdollImpulse;
		projectile.RagdollUpwardImpulse = RagdollUpwardImpulse;
		projectile.MaxPiercedEnemies = MaxPiercedEnemies;
		projectile.DamageFalloffPerPierce = DamageFalloffPerPierce;
		projectile.IsMarkedDeckCard = isMarkedDeckCard;
		projectile.LoadedHandCard = isMarkedDeckCard ? SelectedLoadedHandCard : LoadedHandCard.Eye;
		projectile.PhysicalDamageMultiplier = GetMarkedDeckPhysicalDamageMultiplier( isMarkedDeckCard );
	}

	[Rpc.Host]
	void RequestShuffleDash( Vector3 direction )
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( direction.Length <= 0.01f )
			return;

		var dashDirection = direction.WithZ( 0f ).Normal;
		var targetPosition = GetShuffleDashTargetPosition( dashDirection );
		WorldPosition = targetPosition;
		ShuffleDashBuffTime = ShuffleDashBuffDuration;
	}

	public void ApplySkillStubDamage( PlayerCombat target, Vector3 hitPosition )
	{
		if ( !Networking.IsHost || !target.IsValid() || target.IsDead )
			return;

		var amount = target.ApplyCardveilDamageBonus( SkillStubBaseDamage );
		if ( target.TryTriggerLuckyCut( out var luckyCutDamage ) )
		{
			amount += luckyCutDamage;
		}

		target.ApplyDamage( new DamageEvent( GameObject, amount, DamageType.Magic, hitPosition ) );
	}

	public void RegisterProjectileHit( int attackId, PlayerCombat target )
	{
		if ( !Networking.IsHost || !target.IsValid() )
			return;

		if ( !AttackCardHits.TryGetValue( attackId, out var targetCardHits ) )
		{
			targetCardHits = new Dictionary<PlayerCombat, int>();
			AttackCardHits[attackId] = targetCardHits;
		}

		AddTargetCardHit( targetCardHits, target );
		if ( targetCardHits[target] is 2 or 3 )
			target.ApplyBleedLuckStacks( 1 );
	}

	public float ScaleCardDamage( float baseDamage, DamageType damageType )
	{
		return Combat.IsValid() ? Combat.ScaleDamage( baseDamage, damageType ) : baseDamage;
	}

	public void NotifyMarkedDeckHit( PlayerCombat target, LoadedHandCard loadedHandCard )
	{
		if ( !Networking.IsHost || !target.IsValid() || target.IsDead )
			return;

		switch ( loadedHandCard )
		{
			case LoadedHandCard.Eye:
				target.RevealToCardveilOwner( GameObject, MarkedDeckRevealDuration, true );
				break;
			case LoadedHandCard.Blood:
				target.ApplyCardveilBleed( GameObject, BloodCardBleedDamage, BloodCardBleedDuration, BloodCardHealFraction );
				break;
			case LoadedHandCard.Ace:
				if ( target.HasBleedLuck )
				{
					target.ApplyDamage( new DamageEvent( GameObject, AceCardBleedLuckBonusDamage, DamageType.Magic, target.WorldPosition + Vector3.Up * 48f ) );
				}
				break;
		}
	}

	int ConsumeMarkedDeckCardIndex()
	{
		if ( !MarkedDeckReady || MarkedDeckCardIndex < 0 )
			return -1;

		var index = MarkedDeckCardIndex;
		MarkedDeckReady = false;
		PredictedMarkedDeckReady = false;
		MarkedDeckCardIndex = -1;
		NextMarkedDeckPassiveTime = MarkedDeckPassiveInterval;
		return index;
	}

	void UpdateMarkedDeckPassive()
	{
		if ( Combat.IsValid() && Combat.IsDead )
			return;

		if ( MarkedDeckReady )
			return;

		if ( NextMarkedDeckPassiveTime > 0f )
			return;

		MarkedDeckReady = true;
		MarkedDeckCardIndex = Game.Random.Next( 0, 3 );
		SetMarkedDeckLoadedLocal( true );
	}

	[Rpc.Broadcast]
	void SetMarkedDeckLoadedLocal( bool loaded )
	{
		if ( !GameObject.Network.IsOwner )
			return;

		PredictedMarkedDeckReady = loaded;
	}

	[Rpc.Host]
	void RequestSetLoadedHandCard( LoadedHandCard card )
	{
		SelectedLoadedHandCard = card;
		SetLoadedHandCardLocal( card );
	}

	[Rpc.Broadcast]
	void SetLoadedHandCardLocal( LoadedHandCard card )
	{
		if ( GameObject.Network.IsOwner )
			PredictedLoadedHandCard = card;
	}

	float GetMarkedDeckPhysicalDamageMultiplier( bool isMarkedDeckCard )
	{
		if ( !isMarkedDeckCard )
			return 1f;

		return SelectedLoadedHandCard == LoadedHandCard.Blood
			? BloodCardDirectDamageMultiplier
			: MarkedDeckPhysicalDamageMultiplier;
	}

	static LoadedHandCard GetNextLoadedHandCard( LoadedHandCard card )
	{
		return card switch
		{
			LoadedHandCard.Eye => LoadedHandCard.Blood,
			LoadedHandCard.Blood => LoadedHandCard.Ace,
			_ => LoadedHandCard.Eye
		};
	}

	float GetCurrentCardRange()
	{
		return ShuffleDashBuffTime > 0f ? Range * ShuffleDashCardRangeMultiplier : Range;
	}

	float GetCurrentCardSpeed()
	{
		return ShuffleDashBuffTime > 0f ? CardVisualSpeed * ShuffleDashCardSpeedMultiplier : CardVisualSpeed;
	}

	Vector3 GetShuffleDashDirection()
	{
		var camera = GetMainCamera();
		var forward = camera.IsValid() ? camera.WorldRotation.Forward.WithZ( 0f ) : WorldRotation.Forward.WithZ( 0f );
		var right = camera.IsValid() ? camera.WorldRotation.Right.WithZ( 0f ) : WorldRotation.Right.WithZ( 0f );
		if ( forward.Length <= 0.01f )
			forward = WorldRotation.Forward.WithZ( 0f );

		if ( forward.Length <= 0.01f )
			forward = Vector3.Forward;

		if ( right.Length <= 0.01f )
			right = Vector3.Right;

		var direction = Vector3.Zero;
		if ( Input.Down( "Forward" ) )
			direction += forward.Normal;

		if ( Input.Down( "Backward" ) )
			direction -= forward.Normal;

		if ( Input.Down( "Left" ) )
			direction -= right.Normal;

		if ( Input.Down( "Right" ) )
			direction += right.Normal;

		return direction.Length > 0.01f ? direction.Normal : forward.Normal;
	}

	Vector3 GetShuffleDashTargetPosition( Vector3 dashDirection )
	{
		var start = WorldPosition + Vector3.Up * 36f;
		var end = start + dashDirection * ShuffleDashDistance;
		var trace = Scene.Trace
			.Ray( start, end )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "deadplayer" )
			.Run();

		var target = trace.Hit
			? trace.HitPosition - dashDirection * 18f
			: end;

		return new Vector3( target.x, target.y, WorldPosition.z );
	}

	Vector3 GetAimPoint( Vector3 cardOrigin )
	{
		var aimRange = GetCurrentCardRange();
		var camera = GetMainCamera();
		if ( camera.IsValid() )
		{
			var start = camera.WorldPosition;
			var end = start + camera.WorldRotation.Forward * aimRange;
			var trace = Scene.Trace
				.Ray( start, end )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "trigger", "deadplayer" )
				.Run();

			return trace.Hit ? trace.HitPosition : end;
		}

		if ( Controller.IsValid() )
			return cardOrigin + Controller.EyeAngles.Forward.Normal * aimRange;

		return cardOrigin + WorldRotation.Forward.Normal * aimRange;
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

	static void AddTargetCardHit( Dictionary<PlayerCombat, int> targetCardHits, PlayerCombat target )
	{
		if ( !targetCardHits.TryAdd( target, 1 ) )
		{
			targetCardHits[target]++;
		}
	}

	static bool TryFindCombat( GameObject hitObject, out PlayerCombat combat )
	{
		combat = hitObject.Components.Get<PlayerCombat>( FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors );
		return combat.IsValid();
	}
}
