Shader "Hidden/VolumetricLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass // #0: render volumtric light texture
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "VolumetricLight.hlsl"
            ENDHLSL
        }

        Pass // #1: compose to target texture
        {
            Blend One One

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "VolumetricLightCompose.hlsl"
            ENDHLSL
        }

        Pass // #2: compose to target texture with Kawase blur
        {
            Blend One One

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentKawaseBlur

            #include "VolumetricLightCompose.hlsl"
            ENDHLSL
        }

        Pass // #3: compose to temp texture with Kawase blur
        {
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentKawaseBlur

            #include "VolumetricLightCompose.hlsl"
            ENDHLSL
        }
    }
}
