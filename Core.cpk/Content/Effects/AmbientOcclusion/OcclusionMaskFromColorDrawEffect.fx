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
    // discard color information
    result.rgb = 0;
    // reduce opacity of "shadow"
    result.a = min(result.a, 0.75);

    return result;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};