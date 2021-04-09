#ifndef THOUSAND_ANT_SIMPLE_LIT_PASS
#define THOUSAND_ANT_SIMPLE_LIT_PASS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct MeshData 
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
}

struct Interpolators
{
    float2 uv                       : TEXCOORD0;
    float3 posWS                    : TEXCOORD2;    // xyz: posWS

    #ifdef _NORMALMAP
        half4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
        half4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
        half4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
    #else
        half3  normal                  : TEXCOORD3;
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        half4 fogFactorAndVertexLight  : TEXCOORD6; // x: fogFactor, yzw: vertex light
    #else
        half  fogFactor                 : TEXCOORD6;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord             : TEXCOORD7;
    #endif

    float4 positionCS                  : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
}

StructuredBuffer<float4x4> _Matrices;   // This will store the LocalToWorld Matrix

Interpolators IndirectLitPassVertex(Attributes input, uint instanceID : SV_INSTANCEID)
{
    float4x4 ltw = _Matrices[instanceID];

    Varyings o = (Varyings)0;

    return o;
}

float4 IndirectLitPassFragment(Interpolators input) : SV_TARGET 
{
    return 1;
}

#endif
