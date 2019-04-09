#define MASK_LAYER_TWO

#include "GroundTileBlendBase.fx"

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};