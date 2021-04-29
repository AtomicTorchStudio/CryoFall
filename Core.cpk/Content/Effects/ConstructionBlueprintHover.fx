#include "Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;
float4 ColorAdd = float4(0, 0, 0, 0);
float4 ColorMultiply = float4(1, 1, 1, 1);
float Time;

float OutlineSize = 0.01;
float4 ColorOutline = float4(0.4, 0.8, 1.0, 1);

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
    float4 color = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

    // de-premultiply alpha
    color.rgb /= color.a;
    
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
        color += ColorAdd;
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