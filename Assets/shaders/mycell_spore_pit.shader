HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "Mycell Spore Pit Dome";
	Version = 1;
}

MODES
{
	Forward();
}

FEATURES
{
	#include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"

	#define S_TRANSLUCENT 1

	float3 g_vToxicColor < UiType( Color ); Default3( 0.24, 1.0, 0.02 ); UiGroup( "Spore Pit,10/Color,10/1" ); >;
	Float3Attribute( g_vToxicColor, g_vToxicColor );

	float g_flOpacity < Default( 0.28 ); Range( 0.0, 1.0 ); UiGroup( "Spore Pit,10/Opacity,10/1" ); >;
	FloatAttribute( g_flOpacity, g_flOpacity );

	float g_flEdgeBoost < Default( 0.34 ); Range( 0.0, 1.0 ); UiGroup( "Spore Pit,10/Opacity,10/2" ); >;
	FloatAttribute( g_flEdgeBoost, g_flEdgeBoost );

	float g_flNoiseScale < Default( 8.0 ); Range( 0.1, 40.0 ); UiGroup( "Spore Pit,10/Noise,10/1" ); >;
	FloatAttribute( g_flNoiseScale, g_flNoiseScale );

	float g_flNoiseSpeed < Default( 0.16 ); Range( -5.0, 5.0 ); UiGroup( "Spore Pit,10/Noise,10/2" ); >;
	FloatAttribute( g_flNoiseSpeed, g_flNoiseSpeed );

	float g_flGlow < Default( 1.4 ); Range( 0.0, 6.0 ); UiGroup( "Spore Pit,10/Glow,10/1" ); >;
	FloatAttribute( g_flGlow, g_flGlow );
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
	#include "common/pixel.hlsl"

	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, false );
	RenderState( DepthEnable, true );
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );

	float HashNoise( float2 p )
	{
		float3 p3 = frac( float3( p.xyx ) * 0.1031 );
		p3 += dot( p3, p3.yzx + 33.33 );
		return frac( (p3.x + p3.y) * p3.z );
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		float2 uv = i.vTextureCoords.xy;
		float time = g_flTime * g_flNoiseSpeed;
		float noiseA = HashNoise( uv * g_flNoiseScale + float2( time, -time * 0.73 ) );
		float noiseB = HashNoise( uv * (g_flNoiseScale * 0.47) + float2( -time * 0.31, time * 0.49 ) );
		float bands = sin( uv.y * 15.0 + uv.x * 4.0 + time * 7.0 ) * 0.5 + 0.5;
		float pattern = saturate( noiseA * 0.46 + noiseB * 0.34 + bands * 0.2 );

		float2 centeredUv = abs( uv - 0.5 ) * 2.0;
		float edge = saturate( max( centeredUv.x, centeredUv.y ) );
		float edgeAlpha = smoothstep( 0.52, 1.0, edge );

		float alpha = saturate( g_flOpacity + edgeAlpha * g_flEdgeBoost + pattern * 0.12 );
		float3 color = g_vToxicColor * (0.7 + pattern * 0.65) * g_flGlow;

		m.Albedo = g_vToxicColor * (0.55 + pattern * 0.35);
		m.Emission = color;
		m.Opacity = alpha;

		return ShadingModelStandard::Shade( m );
	}
}
