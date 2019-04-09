#include "../MainCameraEffect.fxh"

Texture2D SpriteTexture;
const float SpriteTextureScale = 1 / 8.0;

sampler SpriteTextureSampler
{
    Filter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
};

Texture2DArray MaskTextureArray : register(t1);
sampler MaskTextureArraySampler
{
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};

// at least one mask layer should be present
int Mask1ArraySlice;
float2 Mask1Flip;

#ifdef MASK_LAYER_TWO
int Mask2ArraySlice;
float2 Mask2Flip;
#endif

#ifdef MASK_LAYER_THREE
int Mask3ArraySlice;
float2 Mask3Flip;
#endif

#ifdef MASK_LAYER_FOUR
int Mask4ArraySlice;
float2 Mask4Flip;
#endif

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

// Photoshop-like screen blend
float ScreenBlend(float source, float destination)
{
    return destination + source * (1 - destination);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = MainCameraCalculateWorldUV(input.Position.xy);

    float4 color = SpriteTexture.Sample(SpriteTextureSampler, uv * SpriteTextureScale);
		
	// This is a code for flipping UV. 
	// If MaskUVFlip == (1,1) then this code will flip UV horizontally+vertically
	// If MaskUVFlip == (0,0) then no flip will be applied.
	// The same code is used for other layers as well (each layer could have its own mask flip).
    uv = input.TextureCoordinates;
	float2 uvMaskLayer1 = abs(uv - Mask1Flip);
    float mask1 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer1, Mask1ArraySlice)).r;
    color.a = mask1;
    // uncomment this to see the mask
	//return float4(mask1, mask1, mask1, 1);

#ifdef MASK_LAYER_TWO
	float2 uvMaskLayer2 = abs(uv - Mask2Flip);
	float mask2 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer2, Mask2ArraySlice)).r;
    color.a = ScreenBlend(color.a, mask2);
    // uncomment this to see the mask
    //return float4(mask2, mask2, mask2, 1);
#endif

#ifdef MASK_LAYER_THREE
	float2 uvMaskLayer3 = abs(uv - Mask3Flip);
	float mask3 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer3, Mask3ArraySlice)).r;
	color.a = ScreenBlend(color.a, mask3);
    // uncomment this to see the mask
    //return float4(mask3, mask3, mask3, 1);
#endif

#ifdef MASK_LAYER_FOUR
	float2 uvMaskLayer4 = abs(uv - Mask4Flip);
	float mask4 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer4, Mask3ArraySlice)).r;
	color.a = ScreenBlend(color.a, mask4);
    // uncomment this to see the mask
    //return float4(mask4, mask4, mask4, 1);
#endif

    // uncomment to see the combined mask
    //color.rgb = color.rgb * 0.0001 + color.a;
    //color.a = 1;

    // premultiply alpha
    color.rgb *= color.a;
	return color;
};