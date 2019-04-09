// We're reusing BasePostEffect as it contains special full screen vertex shader.
#include "../PostEffects/BasePostEffect.fxh"

// Ambient light fraction effect parameter (value from 0 to 1).
// Value 0 means total darkness in non-lighted areas.
// It's supposed to be provided by the DayNightSystem.
float Ambient = 0;

// Determines how much additive light we want from light sources.
// Please note the additive light value is adjusted accordingly to ambient light value.
// When there is no ambient light (0.0) the MaxAdditiveLightFraction is used,
// when there is full ambient light (1.0) the MinAdditiveLightFraction is used.
float MinAdditiveLightFraction = 0.075;
float MaxAdditiveLightFraction = 0.300;

sampler LightmapSampler : register(s0)
{
    Texture = (LightmapTexture);
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler BaseSampler : register(s1)
{
    Texture = (BaseTexture);
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float4 MainPS(VSOutput input) : COLOR0
{
    float4 color = tex2D(BaseSampler, input.TexCoord);
    float4 light = tex2D(LightmapSampler, input.TexCoord);
    float4 result = float4(color.rgb * Ambient, 1);

    // multiplicative light part
    result.rgb += color.rgb * light.rgb * (1 - Ambient);

    // additive light part
    result.rgb += light.rgb * max(MaxAdditiveLightFraction * (1 - Ambient),
                                  MinAdditiveLightFraction);

    return result;
}