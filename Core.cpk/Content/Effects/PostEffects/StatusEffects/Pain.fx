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
	float vignettingCoef = 0.62 - distance(float2(0.5, 0.5), input.TexCoord);
	vignettingCoef = 7 * vignettingCoef;
	vignettingCoef = clamp(vignettingCoef, 0, 1);
				
	modifiedColor.rgb = lerp(modifiedColor.rgb,
					 		 0.5 * (modifiedColor.rgb + float3(0.667, 0, 0)), 
							 1 - vignettingCoef);

	// calculate pulsation effect
	// pulsation value is in range [0;1]
	float pulsation = (sin(Time * 6) + 1) / 2;
	pulsation = pow(pulsation, 2);
	// remap it to range [0.65;1]
	pulsation = 0.65 + pulsation * 0.35;
	
	Intensity *= pulsation;

    return float4(lerp(originalColor, modifiedColor, Intensity), 1);
}
