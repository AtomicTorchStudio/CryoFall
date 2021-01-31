#include "../Lib.fxh"

Texture2DArray SpriteTexture : register(t0);
int SpriteTextureArraySlice;

// partially based on https://www.reddit.com/r/godot/comments/84fhuh/2d_Outline_shader_finally_got_it_working/
float OutlineSize = 0.02;
const float3 OutlineColor = float3(0.9, 0.15, 0.25);
const float3 OutlineOpacity = 1.0;

sampler SpriteTextureSampler
{
	BorderColor = 0x00000000;
	Filter = LINEAR;
	AddressU = BORDER;
	AddressV = BORDER;
	AddressW = CLAMP;	
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

	float4 color = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));
	color.rgb *= input.Color;
	
	if (color.a < 1)	    
    {
        color.rgb = OutlineColor;

        float maxA = color.a;
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y + OutlineSize, SpriteTextureArraySlice)).a);
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y - OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y,               SpriteTextureArraySlice)).a);
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y,               SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a);
            
		// draw a sharp outline (as we don't want to see shadows from the original sprite glowing)
		maxA = 1 - step(maxA, 0.9);
        color.a = maxA * OutlineOpacity;
    }
    else
    {
        color.a = 0;
    }

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