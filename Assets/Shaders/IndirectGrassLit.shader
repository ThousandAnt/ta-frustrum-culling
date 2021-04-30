Shader "Thousand Ant/Simple Lit Grass Indirect"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" { }
        [MainColor]   _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        _Cutoff ("Alpha Clipping", Range(0.0, 1.0)) = 0.5

        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _SpecGlossMap ("Specular Map", 2D) = "white" { }
        _SmoothnessSource ("Smoothness Source", Float) = 0.0
        _SpecularHighlights ("Specular Highlights", Float) = 1.0

        // -----------------------------------------------------------
        // Vertex Animation Settings
        // -----------------------------------------------------------
        _WorldSize("World Size", Vector) = (1, 1, 1, 1)
        _NoiseMap("Noise Map", 2D) = "white" { }
        _WindSpeed("Wind Speed", Vector) = (1, 1, 1, 1)

        _WaveSpeed("Wave Speed", Float) = 1.0

        _HeightCutoff("Height Cutoff", float) = 1.0

        [HideInInspector] _BumpScale ("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" { }

        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0)
        [NoScaleOffset]_EmissionMap ("Emission Map", 2D) = "white" { }

        // Blending state
        _Surface ("__surface", Float) = 0.0
        _Blend ("__blend", Float) = 0.0
        _Cull ("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip ("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        [ToggleUI] _ReceiveShadows ("Receive Shadows", Float) = 1.0
        // Editmode props
        _QueueOffset ("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex ("BaseMap", 2D) = "white" { }
        [HideInInspector] _Color ("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Shininess ("Smoothness", Float) = 0.0
        [HideInInspector] _GlossinessSource ("GlossinessSource", Float) = 0.0
        [HideInInspector] _SpecSource ("SpecularHighlights", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" { }
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "SimpleLit"
            "ShaderModel" = "4.5"
        }

        Pass
        {
            Name "Grass Indirect Lit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM

            #pragma target 4.5

            // ----------------------------------------------
            // Shader Features
            // ----------------------------------------------
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            // ----------------------------------------------
            // Universal Keywords
            // ----------------------------------------------
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #pragma multi_compile_instancing

            #pragma vertex IndirectLitPassVertex
            #pragma fragment IndirectLitPassFragment

            #include "Core.hlsl"
            #include "IndirectInput.hlsl"
            #include "GrassInput.hlsl"
            #include "Specular.hlsl"
            #include "GrassLitForwardPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster Indirect"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "IndirectInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
