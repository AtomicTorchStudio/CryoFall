#include "../BasePostEffect.fxh"

sampler TextureSampler : register(s0) = sampler_state
{
    Filter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float Intensity;

// Note: this shader effect is partially based on https://www.shadertoy.com/view/4sXSWs
float4 MainPS(VSOutput input) : COLOR0
{    
    // ui is the screen coords (0;0 to 1;1)
    float2 uv = input.TexCoord;

    // this is the color of the framebuffer pixel to which we want to apply the post effect
    float4 c = tex2D(TextureSampler, uv);

    float3 newColor = c.rgb * 2 * float3(1.25, 1.4, 1.5)
                      + float3(0.5, 0.8, 1);

    c.rgb = lerp(c.rgb, newColor, Intensity * 0.667);

    return c;
}