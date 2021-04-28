#ifndef THOUSAND_ANT_SHADOW_CASTER_PASS
#define THOUSAND_ANT_SHADOW_CASTER_PASS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Transforms.hlsl"

float3 _LightDirection;
float3 _LightPosition;

struct MeshData
{
    float4 positionOS: POSITION;
    float3 normalOS: NORMAL;
    float2 texCoord: TEXCOORD0;
};

struct Interpolators
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
};

float4 GetShadowPositionHClip(MeshData input, float4x4 m)
{

    float3 positionWS = TransformObjectToWorld(m, input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(m, input.normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Interpolators ShadowPassVertex(MeshData input, uint instanceID : SV_INSTANCEID) 
{
    float4x4 m = _Matrices[instanceID];

    Interpolators output = (Interpolators)0;
    output.uv = TRANSFORM_TEX(input.texCoord, _BaseMap);
    output.positionCS = GetShadowPositionHClip(input, m);

    return output;
}

half4 ShadowPassFragment(Interpolators input) : SV_TARGET 
{
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    return 0;
}

#endif
