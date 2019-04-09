#include "ColorGradingBase.fxh"

sampler3D TextureLutSampler = LUT_SAMPLER(TextureLut);
float Intensity;

float4 MainPS(VSOutput input) : COLOR0
{       
    float3 originalColor = tex2D(TextureScreenBufferSampler, input.TexCoord).rgb;
    float3 modifiedColor = SampleLut(TextureLutSampler, originalColor);
    return float4(lerp(originalColor, modifiedColor, Intensity), 1);
}