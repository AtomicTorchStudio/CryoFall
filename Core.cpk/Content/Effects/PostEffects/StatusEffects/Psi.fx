#include "../BasePostEffect.fxh"
#include "../../MainCameraEffect.fxh"

// Default sampler (framebuffer)
sampler2D TextureScreenBufferSampler : register(s0) = sampler_state
{
    Texture = <TextureScreenBuffer>;
    Filter = LINEAR;
    AddressW = CLAMP;
    AddressU = CLAMP;
    AddressV = CLAMP;
    // uncomment these lines to adjust the zoom effect
    // with magenta border making it obvious
    BorderColor = 0xFF00FF00;
    AddressU = BORDER;
    AddressV = BORDER;
};

float Intensity;
float Time;

float4 MainPS(VSOutput input) : COLOR0
{
	// the effect should apply higher to the screen bounds
    const float distanceCoef = 0.75;

    const float speed = 3;
    float power = ScreenScale * Intensity / 500;
	
	// please note that the constant was carefully adjusted here by using the BORDER mode
    float zoomCoef = 0.02 * distanceCoef * ScreenScale * Intensity;
    input.TexCoord = input.TexCoord * (1 - zoomCoef) + zoomCoef / 2;

    float2 offset = (float2(cos(Time * speed) * 5, sin(Time * speed) * 3));
    offset *= distanceCoef * power;
    float2 texCoord1 = input.TexCoord + offset;
    float2 texCoord2 = input.TexCoord - offset;

	// sample texture at two locations
    float3 originalColor1 = tex2D(TextureScreenBufferSampler, texCoord1).rgb;
    float3 originalColor2 = tex2D(TextureScreenBufferSampler, texCoord2).rgb;

	// make some channels darker
    originalColor1.r *= 1 - Intensity / 2;
    originalColor2.g *= 1 - Intensity / 2;

	// add some brightness variation
    originalColor1.rgb += 0.15 * Intensity * ((sin(Time * speed * 0.5) + 1) / 2);

	// mix both colors
    float3 result = (originalColor1 + originalColor2) / 2;
    return float4(result, 1);
}
