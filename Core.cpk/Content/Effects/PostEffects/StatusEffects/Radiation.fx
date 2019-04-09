#include "../BasePostEffect.fxh"

sampler TextureSampler : register(s0) = sampler_state
{
    Filter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float Intensity;
float Time;

// Note: this shader effect is partially based on https://www.shadertoy.com/view/4sXSWs
float4 MainPS(VSOutput input) : COLOR0
{    
	// grain & grayscale intensity constants
    float grainStrength = 20.0 * Intensity;
    float grayscaleStrength = 0.6 * Intensity;

    // ui is the screen coords (0;0 to 1;1)
    float2 uv = input.TexCoord;

    // this is the color of the framebuffer pixel to which we want to apply the post effect
    float4 c = tex2D(TextureSampler, uv);

    // prepare random number for pixel (derivative of uv and Time)
    float x = (uv.x + 3) * (uv.y + 4.0) * Time;
    x = fmod((fmod(x, 37.0) + 1.0) * (fmod(x, 127.0) + 1.0), 0.01) - 0.005;

    // apply strength factor
    x *= grainStrength;

    // add grain to result color
    c += x;

    // make color grayscale
    float3 gray = dot(c.rgb, float3(0.3, 0.59, 0.11));
    return float4(c.rgb * (1 - grayscaleStrength) + grayscaleStrength * gray, 1);
}