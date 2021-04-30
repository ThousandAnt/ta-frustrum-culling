#ifndef THOUSANDANT_DEPTH_ONLY_PASS
#define THOUSANDANT_DEPTH_ONLY_PASS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "IndirectInput.hlsl"
#include "Transforms.hlsl"

struct MeshData 
{
    float4 position : POSITION;
    float2 texcoord : TEXCOORD0;
};

struct Interpolators 
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
};

Interpolators DepthOnlyVertex(MeshData input, uint instanceID : SV_INSTANCEID) 
{
    float4x4 m = _Matrices[instanceID];
    Interpolators output = (Interpolators)0;

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionCS = TransformObjectToHClip(m, input.position.xyz);

    return output;
}

half4 DepthOnlyFragment(Interpolators input) : SV_TARGET
{
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    return 0;
}

#endif
