#ifndef THOUSAND_ANT_INDIRECT_LIT_GRASS_INPUT
#define THOUSAND_ANT_INDIRECT_LIT_GRASS_INPUT

#include "IndirectInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
    half4 _WorldSize;
CBUFFER_END

// ---------------------------------------------------------
// Vertex Pass Input Data
// ---------------------------------------------------------
struct MeshData 
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
};

// ---------------------------------------------------------
// Fragment Pass Input Data
// ---------------------------------------------------------
struct Interpolators
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

#ifdef _NORMALMAP
    half4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    half4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    half4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    half3 normal                  : TEXCOORD3;
    half3 viewDi                  : TEXCOORD4;
#endif
    float3 viewDirWS                : TEXCOORD5;

    half4 fogFactorAndVertexLight  : TEXCOORD6; // x: fogFactor, yzw: vertex light
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord             : TEXCOORD7;
#endif

    float4 positionCS              : SV_POSITION;
};

// ---------------------------------------------------------
// Grass Maps
// ---------------------------------------------------------
TEXTURE2D(_WindMap);        SAMPLER(sampler_WindMap);

#endif
