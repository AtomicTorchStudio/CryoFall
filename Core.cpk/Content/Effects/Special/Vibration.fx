#include "../Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;
float Amplitude;
float Speed;
float VerticalStartOffsetRelative; // value from 0 to 1 where 0 is the bottom of the texture

float Time;

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
	uv.x += Amplitude * sin(Time * 100 * Speed) * (1 - VerticalStartOffsetRelative - uv.y);

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