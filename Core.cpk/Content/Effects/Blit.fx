// we're reusing BasePostEffect as it contains special full screen vertex shader
#include "PostEffects/BasePostEffect.fxh"

sampler TextureScreenBufferSampler
{
    Texture = <TextureScreenBuffer>;
    Filter = POINT;
	AddressU = CLAMP;
    AddressV = CLAMP;
};

float4 MainPS(VSOutput input) : COLOR0
{
    return tex2D(TextureScreenBufferSampler, input.TexCoord);
}