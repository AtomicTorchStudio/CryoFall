#include "../BasePostEffect.fxh"
#include "../../MainCameraEffect.fxh"

// Default sampler (framebuffer)
sampler2D TextureScreenBufferSampler : register(s0) = sampler_state
{
    Texture = <TextureScreenBuffer>;
    Filter = POINT;
	AddressU = CLAMP;
    AddressV = CLAMP;
};

float Intensity;
float Time;

float4 MainPS(VSOutput input) : COLOR0
{     
	float2 uv = input.TexCoord;

	// prepare random number for pixel (derivative of uv and Time)
    float x = (uv.x + 3) * (uv.y + 4.0) * Time;
    x = fmod((fmod(x, 37.0) + 1.0) * (fmod(x, 127.0) + 1.0), 0.01) - 0.005;
	x *= 0.25 * Intensity * ScreenScale;
	uv += x;

    float3 originalColor = tex2D(TextureScreenBufferSampler, uv).rgb;
    float3 modifiedColor = originalColor;

	// apply special additive vignetting-like glow effect
	float vignettingCoef = 1.05 - distance(float2(0.5, 0.5), input.TexCoord);
	vignettingCoef = pow(vignettingCoef, 0.667);
	vignettingCoef = 2 * (vignettingCoef - 0.5);
	vignettingCoef = clamp(vignettingCoef, 0, 1);
	vignettingCoef = pow(vignettingCoef, 0.667);
	float additionalColor = 1 - vignettingCoef;
	
	modifiedColor.rgb += 0.75 * additionalColor;
	
	modifiedColor.g *= 0.75;
	modifiedColor.b *= 0.5;

	return float4(lerp(originalColor, modifiedColor, Intensity), 1);
}
