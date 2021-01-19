#include "BasePostEffect.fxh"

// Default sampler (framebuffer)
sampler2D TextureScreenBufferSampler : register(s0) = sampler_state
{
    Texture = <TextureScreenBuffer>;
    Filter = POINT;
	AddressU = CLAMP;
    AddressV = CLAMP;
};

float Intensity;

float4 MainPS(VSOutput input) : COLOR0
{     
    float3 originalColor = tex2D(TextureScreenBufferSampler, input.TexCoord).rgb;
    const float3 modifiedColor = float3(1,1,1);
    return float4(lerp(originalColor, modifiedColor, Intensity), 1);
}
