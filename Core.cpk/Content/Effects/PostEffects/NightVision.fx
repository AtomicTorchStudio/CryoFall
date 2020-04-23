#include "BasePostEffect.fxh"

// Default sampler (framebuffer)
sampler2D TextureScreenBufferSampler : register(s0) = sampler_state
{
    Texture = <TextureScreenBuffer>;
    Filter = POINT;
	AddressU = CLAMP;
    AddressV = CLAMP;
};

float Intensity;
float AdditionalLight;
float Time;

float GetVignettingCoef(float relativeScreenPosition)
{
	float result = 1 - abs(relativeScreenPosition - 0.5) * 2;
	return result;
}

// Note: this shader effect is partially based on https://www.shadertoy.com/view/4sXSWs
float3 ApplyNoise(float3 c, float2 uv, float intensity)
{    
	// grain & grayscale intensity constants
    float grainStrength = 20.0 * intensity;
    	    
    // prepare random number for pixel (derivative of uv and Time)
    float x = (uv.x + 3) * (uv.y + 4.0) * Time;
    x = fmod((fmod(x, 37.0) + 1.0) * (fmod(x, 127.0) + 1.0), 0.01) - 0.005;

    // apply strength factor
    x *= grainStrength;

    // add grain to result color
    c += x;

	return c;
}

float4 MainPS(VSOutput input) : COLOR0
{     
    float3 originalColor = tex2D(TextureScreenBufferSampler, input.TexCoord).rgb;

	// apply noise
    float3 modifiedColor = ApplyNoise(originalColor, input.TexCoord, 0.5);

	// make color grayscale but use only the green component
	modifiedColor.g = dot(modifiedColor.rgb, float3(0.3, 0.59, 0.11));
    modifiedColor.g += AdditionalLight;
	modifiedColor.rb = 0;
		
	// apply vignetting
	float vignettingCoef = 1 - distance(float2(0.5, 0.5), input.TexCoord);
	vignettingCoef = pow(vignettingCoef, 0.667);
	vignettingCoef = 2 * (vignettingCoef - 0.5);
	vignettingCoef = clamp(vignettingCoef, 0, 1);

	modifiedColor.rgb *= pow(vignettingCoef, 0.9);

    return float4(lerp(originalColor, modifiedColor, Intensity), 1);
}
