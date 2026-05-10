namespace Sandbox;

[Title( "Mycell Toxic Pit Volume" )]
[Category( "Battle Royale" )]
public sealed class MycellToxicPitVolume : Component, Component.ITriggerListener
{
	public GameObject Source { get; set; }
	public float Radius { get; set; } = 900f;

	public void OnTriggerEnter( Collider other )
	{
	}

	public void OnTriggerExit( Collider other )
	{
	}

}
