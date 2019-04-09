#include "ColorGradingBase.fxh"

sampler3D TextureLut1Sampler = LUT_SAMPLER(TextureLut1);
sampler3D TextureLut2Sampler = LUT_SAMPLER(TextureLut2);
float BlendFactor;

float4 MainPS(VSOutput input) : COLOR0
{       
    float3 originalColor = tex2D(TextureScreenBufferSampler, input.TexCoord).rgb;
    float3 modifiedColor1 = SampleLut(TextureLut1Sampler, originalColor);
    float3 modifiedColor2 = SampleLut(TextureLut2Sampler, originalColor);
    return float4(lerp(modifiedColor2, modifiedColor1, BlendFactor), 1);
}