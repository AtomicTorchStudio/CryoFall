#include "MainCameraEffect.fxh"

Texture2D SpriteTexture : register(t1);
sampler SpriteTextureSampler
{
    Filter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
};

float4 Color;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = MainCameraCalculateWorldUV(input.Position.xy);
    float4 color = SpriteTexture.Sample(SpriteTextureSampler, uv);
    // de-premultiply alpha
    color.rgb /= color.a;
    
    color *= Color;

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