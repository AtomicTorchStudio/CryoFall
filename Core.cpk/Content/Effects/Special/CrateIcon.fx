#include "../Lib.fxh"

Texture2DArray SpriteTexture : register(t0);
int SpriteTextureArraySlice;

Texture2DArray MaskTexture : register(t1);
int MaskTextureArraySlice;

// partially based on https://www.reddit.com/r/godot/comments/84fhuh/2d_Outline_shader_finally_got_it_working/
const float OutlineSize = 0.05;
const float3 OutlineColor = float3(0, 0, 0);
const float OutlineOpacity = 0.7;

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
	float mask = MaskTexture.Sample(SpriteTextureSampler, float3(uv, MaskTextureArraySlice)).r;
		
	// de-premultiply alpha
	color.rgb /= color.a;

	const float threshold = 0.9;

	if (color.a <= threshold)
    {
        if (   SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y + OutlineSize, SpriteTextureArraySlice)).a > threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y - OutlineSize, SpriteTextureArraySlice)).a > threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y,               SpriteTextureArraySlice)).a > threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y,               SpriteTextureArraySlice)).a > threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a > threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a > threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a > threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a > threshold)
		{
			// outer colored border
            color.rgb = OutlineColor;
            color.a = OutlineOpacity;
		}
    }

	// apply mask pattern
	color.a *= mask;

	// uncomment to test
	//color.rgba += float4(1,0,0,0.5);
		
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