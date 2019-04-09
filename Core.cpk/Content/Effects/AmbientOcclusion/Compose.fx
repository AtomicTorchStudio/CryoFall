// we're reusing BasePostEffect as it contains special full screen vertex shader
#include "../PostEffects/BasePostEffect.fxh"

sampler TextureOcclusionSampler
{
    // pixel perfect sampling (render pixel-at-pixel)
    Filter = POINT;
};

float4 MainPS(VSOutput input) : COLOR0
{
    float4 color = tex2D(TextureOcclusionSampler, input.TexCoord);
    // uncomment this to disable AO layer
    //color.a = 0;

    // extend the "shadow" size
    color.a = pow(color.a, 1.75);
    color.a *= 2;

    // limit maximum blackness of the "shadow"
    color.a = min(0.75, color.a);
    return color;
}