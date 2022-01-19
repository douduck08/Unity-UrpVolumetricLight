#define _MAIN_LIGHT_SHADOWS
#define _MAIN_LIGHT_SHADOWS_CASCADE
// #define _ADDITIONAL_LIGHTS
// #define _ADDITIONAL_LIGHT_SHADOWS

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Blit.hlsl"

TEXTURE2D_X_FLOAT(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

#define SAMPLE_NUMBER 16

float HG(float vdotl, float g) {
    float g2 = g*g;
    return (1 - g2) / (4 * 3.1415926 * pow(1 + g2 - 2 * g * vdotl, 1.5));
}

float4 Fragment(Varyings input) : SV_Target {
    float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv).r;
    depth = Linear01Depth(depth, _ZBufferParams);

    Light mainLight = GetMainLight();
    float3 v = normalize(input.direction);
    float vdotl = dot(v, mainLight.direction);

    float attenuation = 0.0;
    for(int i = 0; i < SAMPLE_NUMBER; ++i) {
        float3 positionWS = _WorldSpaceCameraPos.xyz + input.direction * depth * (i + 0.9) / SAMPLE_NUMBER;
        float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
        attenuation += MainLightRealtimeShadow(shadowCoord) * HG(vdotl, 0.8);
    }
    attenuation /= SAMPLE_NUMBER;

    return float4(mainLight.color * attenuation, 1.0);
}