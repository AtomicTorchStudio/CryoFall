#include "../Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;

SamplerState SpriteTextureSampler
{
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MipLODBias = 0.0f;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float3 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float luminosity = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice)).r;
    float3 result = input.Color.rgb * luminosity;
    return result;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};