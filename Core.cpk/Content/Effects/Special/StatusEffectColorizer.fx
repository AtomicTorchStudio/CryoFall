﻿#include "../Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;

sampler SpriteTextureSampler;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = input.TextureCoordinates;
	float4 result = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

	// de-premultiply alpha
	result.rgb /= result.a;
	
	// apply color
	result.rgb = (1 - result.rgb) * input.Color;
	
	// premultiply alpha
	result.rgb *= result.a;
		    
    return result;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};