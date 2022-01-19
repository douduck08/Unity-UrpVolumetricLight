#include "Blit.hlsl"

TEXTURE2D_FLOAT(_VolumetricLightTexture);
SAMPLER(sampler_VolumetricLightTexture);

float4 Fragment(Varyings input) : SV_Target
{
    float3 color = SAMPLE_TEXTURE2D_X(_VolumetricLightTexture, sampler_VolumetricLightTexture, input.uv).rgb;
    return float4(color, 1.0);
}