#include "Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;
float4 ColorAdd = float4(0, 0, 0, 0);
float4 ColorMultiply = float4(1, 1, 1, 1);
float Time;

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
    float4 result = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

    // de-premultiply alpha
    result.rgb /= result.a;

    result *= ColorMultiply;
    result += ColorAdd;

	result.a = lerp(result.a,
					result.a * (1 + sin(Time * 8.5)) / 2.0,
					0.25);

    // premultiply alpha
    result.rgb *= result.a;
        
    return result;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};