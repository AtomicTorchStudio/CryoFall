#include "Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;

// the only difference with DefaultDrawEffect is that here we have a custom sampler with the transparent border
sampler SpriteTextureSampler
{
    BorderColor = 0x00000000;
    Filter = LINEAR;    
    AddressU = BORDER;
    AddressV = BORDER;
    AddressW = BORDER;
};

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
	result *= input.Color;
	
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