#include "../BasePostEffect.fxh"

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
    float3 originalColor = tex2D(TextureScreenBufferSampler, input.TexCoord).rgb;
    float3 modifiedColor = originalColor;

	// calculate vignetting-like overlay
	float vignettingCoef = 2 * (0.6 - distance(float2(0.5, 0.5), input.TexCoord));
	vignettingCoef = clamp(vignettingCoef, 0, 1);
	vignettingCoef = 5 * vignettingCoef;
	vignettingCoef = clamp(vignettingCoef, 0, 1);
	float additionalColor = 1 - vignettingCoef;
			
	modifiedColor.rgb = lerp(modifiedColor.rgb,
					 		 (modifiedColor.rgb + float3(0.5 * additionalColor, 0, 0)) / 2, 
							 pow(additionalColor, 1.5));

	// calculate pulsation effect
	// pulsation value is in range [0;1]
	float pulsation = (sin(Time * 6) + 1) / 2;
	pulsation = pow(pulsation, 3);
	// remap it to range [0.65;1]
	pulsation = 0.65 + pulsation * 0.35;
	
	Intensity *= pulsation;

    return float4(lerp(originalColor, modifiedColor, Intensity), 1);
}
