#include "Lib.fxh"

Texture2DArray SpriteTexture : register(t0);
int SpriteTextureArraySlice;

sampler SpriteTextureSampler
{
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};

Texture2DArray MaskTextureArray : register(t1);
sampler MaskTextureArraySampler
{
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};

int MaskArraySlice;

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
    
    float mask = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uv, MaskArraySlice)).r;
    color.a *= mask;

    // uncomment to see the mask
    //color.rgb = color.rgb * 0.0001 + color.a;
    //color.a = 1;

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