#include "Lib.fxh"

float ScreenScale = 1;
float2 CameraWorldOffset;

float2 MainCameraCalculateScreenRelativePosition(float2 position)
{
    return (position / (ONE_TILE_REAL_SIZE * ScreenScale));
}

float2 MainCameraCalculateWorldUV(float2 position)
{
    float2 screenRelativePosition = MainCameraCalculateScreenRelativePosition(position);
    return screenRelativePosition + CameraWorldOffset * float2(1, -1);
}