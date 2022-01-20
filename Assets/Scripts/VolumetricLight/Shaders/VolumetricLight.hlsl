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

TEXTURE2D_X_FLOAT(_BlueNoise);
SAMPLER(sampler_BlueNoise);
float4 _BlueNoise_TexelSize;

int _SampleNumber;
float _Scattering;
float _RandomStrength;
float4 _TexelSize;

float HG(float vdotl, float g) {
    // Henyey-Greenstein scattering function
    float g2 = g*g;
    return (1 - g2) / (4 * 3.1415926 * pow(1 + g2 - 2 * g * vdotl, 1.5));
}

float GetMainLightAttenuation(float3 positionWS) {
    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    return MainLightRealtimeShadow(shadowCoord);
}

float4 Fragment(Varyings input) : SV_Target {
    float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv).r;

    // blue noise
    float2 noiseUv = input.uv * _TexelSize.zw * _BlueNoise_TexelSize.xy;
    float blueNoise = SAMPLE_TEXTURE2D_LOD(_BlueNoise, sampler_BlueNoise, noiseUv, 0).a;

    Light mainLight = GetMainLight();
    float3 v = normalize(input.direction);
    float vdotl = dot(v, mainLight.direction);
    float intensity = HG(vdotl, _Scattering) * length(input.direction);

    float farDst = Linear01Depth(depth, _ZBufferParams);
    float stepSize = farDst / _SampleNumber;
    float sampleDst = lerp(0.5, blueNoise, _RandomStrength) * stepSize;
    float3 attenuation = 0.0;
    for(int i = 0; i < _SampleNumber; ++i) {
        float3 positionWS = _WorldSpaceCameraPos.xyz + input.direction * sampleDst;
        attenuation += GetMainLightAttenuation(positionWS) * stepSize * intensity * mainLight.color;
        sampleDst += stepSize;
    }

    return float4(attenuation, 1.0);
}