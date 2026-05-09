namespace Sandbox;

public enum DamageType
{
	Physical,
	Magic
}

public enum BuffType
{
	PhysicalDamagePercent,
	MagicDamagePercent,
	MaxHealth,
	CooldownReductionPercent,
	AmmoCapacity
}

public enum CharacterId
{
	None,
	Cardveil,
	ClubBrawler
}

public enum LoadedHandCard
{
	Eye,
	Blood,
	Ace
}

public readonly struct DamageEvent
{
	public DamageEvent( GameObject source, float amount, DamageType damageType, Vector3 hitPosition, Vector3 impulse = default )
	{
		Source = source;
		Amount = amount;
		DamageType = damageType;
		HitPosition = hitPosition;
		Impulse = impulse;
	}

	public GameObject Source { get; }
	public float Amount { get; }
	public DamageType DamageType { get; }
	public Vector3 HitPosition { get; }
	public Vector3 Impulse { get; }
}
