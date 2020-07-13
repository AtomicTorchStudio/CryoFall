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
    const float distanceCoef = 1;
	const float speed = 1.5;
	
	float power = Intensity * ScreenScale / 300;

	// sin range is [-1;1] and we will visualize only values in rage [0;1]
	// so we can use this as a interval
    float powerMultiplier = sin(Time * speed / 3);
    const float intervalDecrease = 0.5; // use this constant to adjust the interval of the effect
    powerMultiplier -= intervalDecrease;
    powerMultiplier *= 1 / (1 - intervalDecrease); // normalize range so it could reach 1 max value
	powerMultiplier = clamp(powerMultiplier, 0, 1);
	powerMultiplier = pow(powerMultiplier, 0.667);
	
    if (powerMultiplier < 0.025)
    {
        powerMultiplier = 0;
    }
	
    // please note that the constant was carefully adjusted here by using the BORDER mode
    float zoomCoef = 0.035 * distanceCoef * ScreenScale * powerMultiplier;
    input.TexCoord = input.TexCoord * (1 - zoomCoef) + zoomCoef / 2;

    power *= powerMultiplier;

	float2 offset = (float2(cos(Time * speed - 1) * 5, sin(Time * speed - 1) * 4));
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
