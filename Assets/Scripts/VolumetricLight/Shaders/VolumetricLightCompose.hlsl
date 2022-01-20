#include "Blit.hlsl"

TEXTURE2D_FLOAT(_VolumetricLightTexture);
SAMPLER(sampler_VolumetricLightTexture);
float4 _VolumetricLightTexture_TexelSize;

float4 Fragment(Varyings input) : SV_Target
{
    float3 color = SAMPLE_TEXTURE2D_X(_VolumetricLightTexture, sampler_VolumetricLightTexture, input.uv).rgb;
    return float4(color, 1.0);
}

float4 FragmentKawaseBlur(Varyings input) : SV_Target
{
    float4 offset = _VolumetricLightTexture_TexelSize.xyxy * float4(0.5, 0.5, -0.5, -0.5);

    float3 color = SAMPLE_TEXTURE2D_X(_VolumetricLightTexture, sampler_VolumetricLightTexture, input.uv + offset.xy).rgb;
    color += SAMPLE_TEXTURE2D_X(_VolumetricLightTexture, sampler_VolumetricLightTexture, input.uv + offset.xw).rgb;
    color += SAMPLE_TEXTURE2D_X(_VolumetricLightTexture, sampler_VolumetricLightTexture, input.uv + offset.zy).rgb;
    color += SAMPLE_TEXTURE2D_X(_VolumetricLightTexture, sampler_VolumetricLightTexture, input.uv + offset.zw).rgb;
    return float4(color * 0.25, 1.0);
}