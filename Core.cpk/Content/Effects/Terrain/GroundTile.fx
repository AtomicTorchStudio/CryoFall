#include "../MainCameraEffect.fxh"

Texture2D SpriteTexture;
const float SpriteTextureScale = 1 / 8.0;
float4x4 WorldViewProjection;
sampler SpriteTextureSampler;

struct VSVertexInput
{
    float2 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VSInstanceInput
{
    float2 Position : POSITION1;    
};

struct VSOutput
{
    float4 Position : SV_POSITION;
};

VSOutput MainVS(in VSVertexInput vertexInput, VSInstanceInput instanceInput)
{
    VSOutput output;

    output.Position = float4(vertexInput.Position + instanceInput.Position, 1, 1);
    output.Position = mul(output.Position, WorldViewProjection);
        
    return output;
}

float4 MainPS(VSOutput input) : COLOR
{
    float2 uv = MainCameraCalculateWorldUV(input.Position.xy);

    float4 result = SpriteTexture.Sample(SpriteTextureSampler, uv * SpriteTextureScale);
    return result;
}

technique GroundTileDrawing
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};