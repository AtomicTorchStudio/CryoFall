#include "../Lib.fxh"

Texture2DArray SpriteTexture : register(t0);
int SpriteTextureArraySlice;

// partially based on https://www.reddit.com/r/godot/comments/84fhuh/2d_Outline_shader_finally_got_it_working/
float OutlineSize = 0.02;
float4 ColorOutline = float4(0.9, 0.15, 0.25, 1);
float4 ColorMultiply = float4(0.25, 1.0, 0.5, 0.5);
float Time;

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
	
	if (color.a < 1)	    
    {
        // outline effect
        color.rgb = ColorOutline.rgb;

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
        color.a = maxA * ColorOutline.a;
    }
    else
    {
        color *= ColorMultiply;
    }
    
    // flickering animation
    color.a = lerp(color.a,
        		   color.a * (1 + sin(Time * 8.5)) / 2.0,
        		   0.25);

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