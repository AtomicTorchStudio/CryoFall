#include "Lib.fxh"
#include "MainCameraEffect.fxh"


float4x4 MatrixTransform;


Texture2DArray SpriteTexture;
int SpriteTextureArraySlice;

sampler SpriteTextureSampler;

float Power = 0.15;
float Speed = 0.333;
float PivotY = 0.2;
int PhaseOffset = 0;
int Flip = 0;

float Time;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

// wave generation functions based on https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch16.html
float SmoothCurve( float x )
{
	return x * x *( 3.0 - 2.0 * x );
}

float TriangleWave( float x )
{
	return abs( frac( x + 0.5 ) * 2.0 - 1.0 );
}

float SmoothTriangleWave( float x )
{
	return SmoothCurve( TriangleWave( x ) );
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = input.TextureCoordinates;
	//return float4(uv.x, uv.y, 0, 1);
	
	float t = Time * Speed + PhaseOffset * 0.05;
	// calculate uv offset power - depends on the Y value of uv
	float uvPowerCoef = Power * smoothstep(1 - PivotY, 0, uv.y);
	
	// calculate uv offset
	//float uvOffset = sin(t); // sin wave is too simple to make compelling wind simulation
	float uvOffset = (SmoothTriangleWave(t) - 0.25) * 0.5;
	// apply grass move animation
	uv.x += uvPowerCoef * uvOffset;
	
	// apply flipping
	uv.x = clamp(uv.x, 0, 1);
	uv.x = abs(uv.x - Flip);

	//return float4(uv.x, uv.y, 0, 1);

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