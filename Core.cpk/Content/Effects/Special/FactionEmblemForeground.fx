#include "../Lib.fxh"

Texture2DArray SpriteTexture : register(t0);
int SpriteTextureArraySlice;

// partially based on https://www.reddit.com/r/godot/comments/84fhuh/2d_Outline_shader_finally_got_it_working/
const float OutlineSize = 0.006;
const float3 OutlineColor = float3(0, 0, 0);
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
	// de-premultiply alpha
	color.rgb /= color.a;
	color.rgb *= input.Color;
	
	// stroke effect is disabled - we're not using it as the outline is painted right in the texture
	/*if (color.a < 1)	    
    {
        color.rgb = lerp(OutlineColor, color.rgb, color.a);

        float maxA = color.a;
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y + OutlineSize, SpriteTextureArraySlice)).a);
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x,                uv.y - OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y,               SpriteTextureArraySlice)).a);
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y,               SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x - OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y + OutlineSize, SpriteTextureArraySlice)).a); 
        maxA = max(maxA, SpriteTexture.Sample(SpriteTextureSampler, float3(uv.x + OutlineSize,  uv.y - OutlineSize, SpriteTextureArraySlice)).a);
            
		// draw an outline
        color.a = maxA * OutlineOpacity;
    }	*/

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