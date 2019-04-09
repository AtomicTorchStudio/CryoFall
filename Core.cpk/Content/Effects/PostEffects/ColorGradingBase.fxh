#include "BasePostEffect.fxh"

// Define for LUT sampler
#define LUT_SAMPLER(name) \
sampler_state \
{ \
    Texture = <name>; \
    Filter = LINEAR; \
    AddressU = CLAMP; \
    AddressV = CLAMP; \
    AddressW = CLAMP; \
}

// Default sampler (framebuffer)
sampler2D TextureScreenBufferSampler : register(s0) = sampler_state
{
    Texture = <TextureScreenBuffer>;
    Filter = POINT;
};

// Helper method definition
// We're using 24x24x24 LUT textures only, so we can hardcode these values and improve performance
#define LUT_SIZE 24
static half3 LUT_SCALE = (LUT_SIZE - 1.0) / LUT_SIZE;
static half3 LUT_OFFSET = 1.0 / (2.0 * LUT_SIZE);
float3 SampleLut(sampler3D lut, float3 originalColor)
{
    // see https://developer.nvidia.com/gpugems/GPUGems2/gpugems2_chapter24.html
    return tex3D(lut, LUT_SCALE * originalColor + LUT_OFFSET).rgb;
}