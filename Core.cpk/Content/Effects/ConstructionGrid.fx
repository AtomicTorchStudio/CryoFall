#include "Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;

sampler SpriteTextureSampler;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = input.TextureCoordinates;
	float4 color = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

    // de-premultiply alpha
    color.rgb /= color.a;

	// change color to make neon-blue
	color *= float4(0.4, 0.8, 1, 1);

	// calculate offset from center
	uv = 2 * (0.5 - abs(uv - 0.5));

	// make it round as circle
	color.a *= 1.5 * sin(uv.x) * sin(uv.y);

	// clamp alpha
	color.a = min(color.a, 0.75);
    
    // premultiply alpha
    color.rgb *= color.a;

	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};