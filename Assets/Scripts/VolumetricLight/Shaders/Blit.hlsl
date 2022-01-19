#ifndef BLIT_INCLUDED
#define BLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct Attributes {
    float4 positionOS : POSITION;
    float4 uv : TEXCOORD0;
};

struct Varyings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 direction : TEXCOORD1;
};

float4x4 _FrustumCorners;

float3 GetRayDirection (float2 uv) {
    // world space direction
    float3 direction = _FrustumCorners[0].xyz +
    (_FrustumCorners[3].xyz - _FrustumCorners[0].xyz) * uv.x +
    (_FrustumCorners[1].xyz - _FrustumCorners[0].xyz) * uv.y;
    return direction;
}

Varyings Vertex(Attributes input) {
    Varyings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = input.uv;
    output.direction = GetRayDirection(output.uv);
    return output;
}
#endif