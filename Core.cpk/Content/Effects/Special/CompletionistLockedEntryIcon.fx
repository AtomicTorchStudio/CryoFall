#include "../Lib.fxh"

Texture2DArray SpriteTexture : register(t0);
int SpriteTextureArraySlice;

// partially based on https://www.reddit.com/r/godot/comments/84fhuh/2d_Outline_shader_finally_got_it_working/
const float OutlineSize = 0.02;
const float3 OutlineColor = float3(1.0, 1.0, 1.0);

const float paddingFraction = 0.125;
const float brightness = 0.333;

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

	uv -= paddingFraction / 2;
	uv /= (1 - paddingFraction);

	float4 color = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));
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
            color.a = brightness;
		}
		else
		{
			// transparent outer area
			color.a = 0;
			//color.rgba = float4(1,0,0,1); // debug with red color
		}
    }
	else
	{
		 if (  SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y + OutlineSize, SpriteTextureArraySlice)).a <= threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y - OutlineSize, SpriteTextureArraySlice)).a <= threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y,               SpriteTextureArraySlice)).a <= threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y,               SpriteTextureArraySlice)).a <= threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a <= threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a <= threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a <= threshold
            || SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a <= threshold)
		{
			// inner transparent border
            color.a = 0;
		}
		else
		{
			// content (colorless)
			color.rgb = OutlineColor;
            color.a = brightness;
		}
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