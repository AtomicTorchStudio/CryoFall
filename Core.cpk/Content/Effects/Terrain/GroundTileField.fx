#include "../MainCameraEffect.fxh"

Texture2D SpriteTexture;
const float SpriteTextureScale = 1 / 8.0;

sampler SpriteTextureSampler
{
    Filter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = MainCameraCalculateWorldUV(input.Position.xy);
    float4 result = SpriteTexture.Sample(SpriteTextureSampler, uv * SpriteTextureScale);
    return result;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};