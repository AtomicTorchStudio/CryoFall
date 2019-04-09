#include "Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;
int SpriteTextureArraySliceNext;
float SpriteLerp;

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
	float4 colorA = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));
	float4 colorB = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySliceNext));

    return lerp(colorA, colorB, SpriteLerp);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};