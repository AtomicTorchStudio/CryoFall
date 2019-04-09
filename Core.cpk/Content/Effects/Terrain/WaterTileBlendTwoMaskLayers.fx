#define MASK_LAYER_ONE
#define MASK_LAYER_TWO

#include "WaterTileBase.fx"

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};