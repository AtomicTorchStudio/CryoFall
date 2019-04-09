#include "ColorGradingBase.fxh"

sampler3D TextureLut1Sampler = LUT_SAMPLER(TextureLut1);
sampler3D TextureLut2Sampler = LUT_SAMPLER(TextureLut2);

sampler2D LightmapSampler
{
    Texture = (LightmapTexture);
    Filter = POINT;    
};

float BlendFactor;

float4 MainPS(VSOutput input) : COLOR0
{       
    half3 lightColor = tex2D(LightmapSampler, input.TexCoord).rgb;
    float3 originalColor = tex2D(TextureScreenBufferSampler, input.TexCoord).rgb;
    float3 modifiedColor1 = SampleLut(TextureLut1Sampler, originalColor);
    float3 modifiedColor2 = SampleLut(TextureLut2Sampler, originalColor);
    float3 lutLookupResult = lerp(modifiedColor2, modifiedColor1, BlendFactor);

    // calculate light fraction in the area
    half lightFraction = max(lightColor.r, max(lightColor.g, lightColor.b));
    
    // display original color in lighted area
    float3 result = lerp(lutLookupResult, originalColor, lightFraction);
    
    return float4(result, 1);
}