Shader "Hidden/VolumetricLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "VolumetricLight.hlsl"
            ENDHLSL
        }

        Pass
        {
            Blend One One

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "VolumetricLightCompose.hlsl"
            ENDHLSL
        }
    }
}
