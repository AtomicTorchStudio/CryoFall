#include "../MainCameraEffect.fxh"

Texture2D UnderWaterTexture : register(t0);
#ifdef MASK_LAYER_ONE // if at least one mask is present
Texture2DArray MaskTextureArray : register(t1);
#endif
Texture2D WaterTexture : register(t2);

float Time;

// Water parameters
float3 WaterColor = float3(0, 0.35, 0.65);
float WaterColorMix = 0.75;
float WaterSpeed = 2.5;
float WaterOpacity = 0.85;
float WaterAmplitude = 0.075;
float WaterDiffractionSpeed = 0.1;
float WaterDiffractionFrequency = 8.0;

const float UnderWaterTextureScale = 1 / 8.0;

const float WaterTextureScale = 1 / 8.0;

// Mask parameters (shore line)
// We keep the mask amplitude reasonably low to avoid the mask texture clamping artifacts
const float ShoreMaskAmplitude = 0.15;
const float ShoreMaskSpeed = 0.2; // Please note: this parameter is set from ProtoTileWater
const float ShoreMaskFrequency = 1.25;

#ifdef MASK_LAYER_ONE
int Mask1ArraySlice;
float2 Mask1Flip;
#endif

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

sampler UnderWaterTextureSampler
{
    Filter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
};

sampler MaskTextureArraySampler
{
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float2 ShiftUV(float2 p, float speed, float frequency)
{
    float d = Time * speed;
    float2 f = frequency * (p + d);
    float2 q = cos(float2(cos(f.x - f.y) * cos(f.y),
                          sin(f.x + f.y) * sin(f.y)));
    return q;
}

float2 ShiftUV(float2 uv, float amplitude, float speed, float frequency)
{
    // calculate UV offset to simulate the refraction effect (based on https://www.shadertoy.com/view/Mls3DH)
    float2 p = ShiftUV(uv, speed, frequency);
    float2 q = ShiftUV(uv + 1, speed, frequency);
    return amplitude * (p - q);
}

float2 CalculateMaskOffsetWithFlip(float2 uv, float2 flip)
{
    // we need to recalculate mask offset to match the mask sprite flipping
    // please note that flip is binary (if it's 1.0 then flip)
    return float2(flip.x == 1.0 ? uv.x : -uv.x,
                  flip.y == 1.0 ? uv.y : -uv.y);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // calculate UV coordinates for the water bottom sprite
    float2 uvWorld = MainCameraCalculateWorldUV(input.Position.xy);
    float2 uvWorldOffset = ShiftUV(uvWorld, WaterAmplitude, WaterDiffractionSpeed, WaterDiffractionFrequency);
    float2 uvSprite = uvWorld + uvWorldOffset;
    float4 color = UnderWaterTexture.Sample(UnderWaterTextureSampler, float2(uvSprite.x * UnderWaterTextureScale, uvSprite.y * UnderWaterTextureScale));
    
    // get the water color
    // first, prepare the water surface color (without actual color, only "bumpness")
    uvWorldOffset *= 3;
    float2 uvWater = uvWorld + uvWorldOffset * 0.5 + WaterSpeed * float2(Time / 44, Time / -24);
    float2 uvWater2 = uvWorld - uvWorldOffset * 0.667 + WaterSpeed * float2(Time / -29, Time / 41) + float2(1.333, -0.667);
    float2 uvWater3 = uvWorld - uvWorldOffset * 0.333 + WaterSpeed * float2(Time / 36, Time / 37) + float2(-4.667, 2.333);
    float3 waterColor = WaterTexture.Sample(UnderWaterTextureSampler, WaterTextureScale * uvWater).rgb;
    float3 waterColor2 = WaterTexture.Sample(UnderWaterTextureSampler, WaterTextureScale * uvWater2).rgb;
    float3 waterColor3 = WaterTexture.Sample(UnderWaterTextureSampler, WaterTextureScale * uvWater3).rgb;
    waterColor = (waterColor + waterColor2 + waterColor3) / 3;
    // mix in the water color
    waterColor = lerp(waterColor, WaterColor, WaterColorMix);

    // mix the result water color with the underlying texture color
    color.rgb = lerp(color.rgb, waterColor.rgb, WaterOpacity);
    color.a = 1;
   
	// This is a code for flipping UV. 
	// If MaskUVFlip == (1,1) then this code will flip UV horizontally+vertically
	// If MaskUVFlip == (0,0) then no flip will be applied.
	// The same code is used for other layers as well (each layer could have its own mask flip).
#ifdef MASK_LAYER_ONE
    float2 uvMask = input.TextureCoordinates;
    float2 uvMaskOffset = ShiftUV(uvWorld, ShoreMaskAmplitude, ShoreMaskSpeed, ShoreMaskFrequency);

    float2 uvMaskLayer1 = abs(uvMask - Mask1Flip);
    uvMaskLayer1 += CalculateMaskOffsetWithFlip(uvMaskOffset, Mask1Flip);
	float mask1 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer1, Mask1ArraySlice)).r;
	color.a = mask1;
    // uncomment this to see the mask
	//return float4(mask1, mask1, mask1, 1);
#endif

#ifdef MASK_LAYER_TWO
	float2 uvMaskLayer2 = abs(uvMask - Mask2Flip);
    uvMaskLayer2 += CalculateMaskOffsetWithFlip(uvMaskOffset, Mask2Flip);
	float mask2 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer2, Mask2ArraySlice)).r;
	color.a = max(color.a, mask2);
#endif

#ifdef MASK_LAYER_THREE
	float2 uvMaskLayer3 = abs(uvMask - Mask3Flip);
    uvMaskLayer3 += CalculateMaskOffsetWithFlip(uvMaskOffset, Mask3Flip);
	float mask3 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer3, Mask3ArraySlice)).r;
	color.a = max(color.a, mask3);
#endif

#ifdef MASK_LAYER_FOUR
	float2 uvMaskLayer4 = abs(uvMask - Mask4Flip);
    uvMaskLayer4 += CalculateMaskOffsetWithFlip(uvMaskOffset, Mask4Flip);
	float mask4 = MaskTextureArray.Sample(MaskTextureArraySampler, float3(uvMaskLayer4, Mask3ArraySlice)).r;
	color.a = max(color.a, mask4);
#endif

    // uncomment to see the combined mask
    //color.rgb = color.rgb * 0.0001 + color.a;
    //color.a = 1;

    // premultiply alpha
    color.rgb *= clamp(color.a, 0, 1);
        
    return color;
};