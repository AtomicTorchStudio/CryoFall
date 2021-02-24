#include "../Lib.fxh"

Texture2DArray SpriteTexture : register(t0);
Texture2DArray NoiseTexture : register(t1);
int SpriteTextureArraySlice;

sampler SpriteTextureSampler : register(s0);

sampler NoiseTextureSampler : register(s1) = sampler_state
{
    Filter = LINEAR;
	AddressU = WRAP;
    AddressV = WRAP;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float Time;
float Progress;
float2 NoiseTextureUvScale;

const float3 ColorGlow = 1.333 * float3(0.9, 0.6, 1.0);
const float GlowPowerCoef = 1.5;
const float TransparentEdgeHeight = 0.5;

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = input.TextureCoordinates;
	float4 result = SpriteTexture.Sample(SpriteTextureSampler, float3(uv, SpriteTextureArraySlice));

	// de-premultiply alpha
	result.rgb /= result.a;
	 
	float2 uvNoise = uv * NoiseTextureUvScale * 0.3;
    float noise = NoiseTexture.Sample(NoiseTextureSampler, float3(uvNoise, 0));
    //return float4(noise,noise,noise, 1); // noise preview 
    
    const float Speed = 3.0; 
    float progress = Progress;
    const float transparentEdgeHeight = TransparentEdgeHeight * Speed;
        
    float threshold = uv.g * transparentEdgeHeight
                      - transparentEdgeHeight
                      + progress * (transparentEdgeHeight + 1);
    threshold = clamp(threshold, 0, 1);
        
    float a = step(noise, threshold);
    //return float4(a, a, a, 1); // mask preview
    
    // add glow
    result.rgb += ColorGlow * clamp(pow(1 - threshold, GlowPowerCoef), 0, 1);
     
    result.a *= a;
    	
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