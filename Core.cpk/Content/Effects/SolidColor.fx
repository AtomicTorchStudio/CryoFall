#include "Lib.fxh"

float4 Color;

struct VertexShaderOutput
{
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};