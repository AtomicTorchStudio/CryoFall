#include "../Lib.fxh"

Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;
float Time;
const float Speed = 2;
const float LengthScale = 0.5;

float Opacity;
float Length;
float FadeInFraction = 0.1;
float FadeOutFraction = 0.1;

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
    uv.x = uv.x * Length * LengthScale;
    //uv.x -= Time * Speed;
    float4 result = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

	// de-premultiply alpha
    result.rgb /= result.a;
	
	// apply opacity
    result.a *= pow(Opacity, 0.333);
    
    // apply fade in/out
    float fractionCoef = min(smoothstep(0, FadeInFraction, input.TextureCoordinates.x),
                             smoothstep(0, FadeOutFraction, 1 - input.TextureCoordinates.x));
    
    result.a *= min(fractionCoef, 1);
	
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