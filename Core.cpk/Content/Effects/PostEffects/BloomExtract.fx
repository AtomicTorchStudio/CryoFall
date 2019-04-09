// Pixel shader extracts the brighter areas of an image.
// This is the first step in applying a bloom postprocess.

#include "BasePostEffect.fxh"

sampler TextureSampler : register(s0);

float BloomThreshold;

float4 MainPS(VSOutput input) : COLOR0
{    
    float4 c = tex2D(TextureSampler, input.TexCoord);
	// adjust it to keep only values brighter than the specified threshold.
    return saturate((c - BloomThreshold) / (1 - BloomThreshold)); 
}