#include "../Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;

SamplerState SpriteTextureSampler;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = input.TextureCoordinates;
	float4 result = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));
    result.a = 1 - result.r; // black color is mask, white color is unmask
    // discard color information
    result.rgb = 0;

    return result;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};