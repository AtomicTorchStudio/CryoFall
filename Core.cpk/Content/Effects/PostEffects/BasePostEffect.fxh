#include "../Lib.fxh"

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

// Draw as single triangle - trick from https://www.slideshare.net/DevCentralAMD/vertex-shader-tricks-bill-bilodeau
VSOutput MainVS(uint id : SV_VertexID)
{
    VSOutput output;

    // generate clip space position
    output.Position.x = (float) (id / 2) * 4.0 - 1.0;
    output.Position.y = (float) (id % 2) * 4.0 - 1.0;
    output.Position.z = 0.0;
    output.Position.w = 1.0;

    // texture coordinates
    output.TexCoord.x = (float) (id / 2) * 2.0;
    output.TexCoord.y = 1.0 - (float) (id % 2) * 2.0;

    return output;
}

technique FullscreenPostEffect
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};