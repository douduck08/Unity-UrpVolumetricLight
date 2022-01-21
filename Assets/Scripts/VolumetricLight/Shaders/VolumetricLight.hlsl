#define _MAIN_LIGHT_SHADOWS
#define _MAIN_LIGHT_SHADOWS_CASCADE
#define _ADDITIONAL_LIGHTS
#define _ADDITIONAL_LIGHT_SHADOWS

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
float _RandomStrength;
float _Scattering;
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
    // blue noise
    float2 noiseUv = input.uv * _TexelSize.zw * _BlueNoise_TexelSize.xy;
    float blueNoise = SAMPLE_TEXTURE2D_LOD(_BlueNoise, sampler_BlueNoise, noiseUv, 0).a;

    // prepare ray marching
    float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv).r;
    float farDst = Linear01Depth(depth, _ZBufferParams);
    float stepSize = farDst / _SampleNumber;
    float sampleDst = lerp(0.5, blueNoise, _RandomStrength) * stepSize;

    // light data
    Light mainLight = GetMainLight();
    float3 v = normalize(input.direction);
    float mainVdotl = dot(v, mainLight.direction);
    float mainScattering = HG(mainVdotl, _Scattering);
    uint pixelLightCount = _AdditionalLightsCount.x;
    
    // ray marching
    float3 attenuation = 0.0;
    for(int i = 0; i < _SampleNumber; ++i) {
        float3 positionWS = _WorldSpaceCameraPos.xyz + input.direction * sampleDst;
        attenuation += (GetMainLightAttenuation(positionWS) * stepSize * mainScattering) * mainLight.color;

        for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex) {
            Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
            float vdotl = dot(v, light.direction);
            float scattering = HG(vdotl, _Scattering);
            attenuation += (light.distanceAttenuation * light.shadowAttenuation * stepSize * scattering) * light.color;
        }

        sampleDst += stepSize;
    }

    float intensity = length(input.direction);
    return float4(attenuation * intensity, 1.0);
}