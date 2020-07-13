#include "../Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;
float Time;
const float Speed = 2;

sampler SpriteTextureSampler
{
    Filter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
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
    uv.x += Time * Speed;
    float4 result = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

	// de-premultiply alpha
    result.rgb /= result.a;
	
	// apply color
    float4 color = input.Color;
    result.rgb = lerp(result.rgb, 
                      result.rgb * color.rgb,
                      1 - pow(result.a, 1.5));
    result.a *= color.a;
	
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