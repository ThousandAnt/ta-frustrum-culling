#ifndef THOUSAND_ANT_HELPERS
#define THOUSAND_ANT_HELPERS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Transforms.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SpaceTransforms.hlsl""

VertexPositionInputs GetVertexPositionInputs(float4x4 matrix, float3 positionOS) 
{
    VertexPositionInputs inputs;

    //  TODO: Implement the Transform functions
    input.positionWS = TransformObjectToWorld(matrix, positionOS);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(matrix, input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;
    return inputs;
}

#endif
