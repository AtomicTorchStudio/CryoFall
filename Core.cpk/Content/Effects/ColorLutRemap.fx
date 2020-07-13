#include "Lib.fxh"

sampler TextureSourceSampler : register(s0) = sampler_state
{
    Texture = <TextureSource>;
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};

sampler3D TextureLutSampler : register(s1) = sampler_state
{
    Texture = <TextureLut>;
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

#define LUT_SIZE 24
static half3 LUT_SCALE = (LUT_SIZE - 1.0) / LUT_SIZE;
static half3 LUT_OFFSET = 1.0 / (2.0 * LUT_SIZE);
float3 SampleLut(sampler3D lut, float3 originalColor)
{
    // see https://developer.nvidia.com/gpugems/GPUGems2/gpugems2_chapter24.html
    return tex3D(lut, LUT_SCALE * originalColor + LUT_OFFSET).rgb;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float4 color = tex2D(TextureSourceSampler, uv);
    
    // de-premultiply alpha
    color.rgb /= color.a;
	
	// modify color     
    color.rgb = SampleLut(TextureLutSampler, color.rgb);
    
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