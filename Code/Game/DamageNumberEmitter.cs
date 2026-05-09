namespace Sandbox;

[Title( "Damage Number Emitter" )]
[Category( "Battle Royale" )]
public sealed class DamageNumberEmitter : Component
{
	[Property] public float Lifetime { get; set; } = 0.85f;
	[Property] public float RiseSpeed { get; set; } = 42f;
	[Property] public Vector2 PanelSize { get; set; } = new( 120f, 64f );
	[Property] public float RenderScale { get; set; } = 0.48f;
	[Property] public float ScaleReferenceDistance { get; set; } = 600f;
	[Property] public float MinRenderScaleMultiplier { get; set; } = 0.9f;
	[Property] public float MaxRenderScaleMultiplier { get; set; } = 3.2f;

	public void Spawn( float amount, DamageType damageType, Vector3 worldPosition )
	{
		if ( amount <= 0f )
			return;

		var popupObject = new GameObject( false, "Damage Number Popup" );
		popupObject.NetworkMode = NetworkMode.Never;
		popupObject.WorldPosition = worldPosition;

		var popup = popupObject.Components.Create<DamageNumberPopup>();
		popup.Amount = amount;
		popup.DamageType = damageType;
		popup.Lifetime = Lifetime;
		popup.RiseSpeed = RiseSpeed;
		popup.PanelSize = PanelSize;
		popup.RenderScale = RenderScale;
		popup.ScaleReferenceDistance = ScaleReferenceDistance;
		popup.MinRenderScaleMultiplier = MinRenderScaleMultiplier;
		popup.MaxRenderScaleMultiplier = MaxRenderScaleMultiplier;

		popupObject.Enabled = true;
	}
}
