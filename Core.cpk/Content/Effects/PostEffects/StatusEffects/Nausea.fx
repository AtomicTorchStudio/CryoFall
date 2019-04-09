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

float GetVignettingCoef(float relativeScreenPosition)
{
	float result = 1 - abs(relativeScreenPosition - 0.5) * 2;
	return result;
}

float4 MainPS(VSOutput input) : COLOR0
{     
	// the effect should apply higher to the screen bounds
	float distanceCoef = 2 * (0.5 - distance(float2(0.5, 0.5), input.TexCoord));
	distanceCoef = clamp(distanceCoef, 0, 1);
	distanceCoef = pow(distanceCoef, 0.667);

	const float speed = 1.5;
	float power = Intensity * ScreenScale / 200;

	float powerMultiplier = sin(Time * speed / 3) - 0.5;
	powerMultiplier = max(0, powerMultiplier);

	power *= powerMultiplier;

	float2 offset = (float2(cos(-Time * speed) * 5, sin(Time * speed) * 2));
	offset *= distanceCoef * power;
	float2 texCoord1 = input.TexCoord + offset;
	float2 texCoord2 = input.TexCoord - offset;

	// sample texture at two locations
    float3 originalColor1 = tex2D(TextureScreenBufferSampler, texCoord1).rgb;
    float3 originalColor2 = tex2D(TextureScreenBufferSampler, texCoord2).rgb;

	// mix both colors
	float3 result = (originalColor1 + originalColor2) / 2;
	return float4(result, 1);    
}
