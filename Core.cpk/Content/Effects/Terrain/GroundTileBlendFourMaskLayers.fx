#define MASK_LAYER_TWO
#define MASK_LAYER_THREE
#define MASK_LAYER_FOUR

#include "GroundTileBlendBase.fx"

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};