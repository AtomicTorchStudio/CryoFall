#include "Lib.fxh"
#include "MainCameraEffect.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;
sampler SpriteTextureSampler
{
    Filter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
};

float Time;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// UV for the whole grid renderer
	float2 uvWholeGrid = input.TextureCoordinates;
	// UV for the individual chunk GPU is rendering now
	float2 uv = MainCameraCalculateWorldUV(input.Position.xy);
	float4 color = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

	// de-premultiply alpha
	color.rgb /= color.a;

	// change color to make neon-blue
	color *= float4(0, 0.7, 1, 1);

	// make it round as circle
	float distanceCoef = 1 - distance(float2(0.5, 0.5), uvWholeGrid) * 2;
	distanceCoef = pow(distanceCoef, 0.7);
	distanceCoef = clamp(distanceCoef, 0, 1);
	color.a *= distanceCoef;

	// apply animation
	color.a = lerp(color.a,
				   color.a * (1 + cos(distanceCoef * 40 + Time * 4))
						   * 0.5,
				   0.5);

	// adjust brightness
	color.a *= 1.2;
	color.a = clamp(color.a, 0, 1);

	// premultiply alpha
	color.rgb *= color.a;

	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};