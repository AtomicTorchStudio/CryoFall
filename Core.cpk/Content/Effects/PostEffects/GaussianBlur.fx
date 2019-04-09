// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

#include "BasePostEffect.fxh"

sampler TextureSampler : register(s0) = sampler_state
{
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

#define SAMPLE_COUNT 15

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

float4 MainPS(VSOutput input) : COLOR0
{
    float4 c = 0;
           
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += tex2D(TextureSampler, input.TexCoord + SampleOffsets[i]) * SampleWeights[i];
    }
       
    return c;
}