namespace Sandbox;

[Title( "Mycell Toxic Pit Post Process" )]
[Category( "Battle Royale" )]
public sealed class MycellToxicPitPostProcess : BasePostProcess<MycellToxicPitPostProcess>
{
	[Property] public float Intensity { get; set; } = 1f;
	[Property] public float VisibleDistance { get; set; } = 500f;
	[Property] public Color FogColor { get; set; } = new( 0.08f, 0.55f, 0.12f, 1f );

	public override void Render()
	{
		// MVP hook point. The current visible screen fog is drawn by BattleRoyaleHud so the ult
		// remains compile-safe while the custom depth shader is still being tuned.
	}
}
