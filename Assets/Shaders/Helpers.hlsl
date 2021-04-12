#ifndef THOUSAND_ANT_HELPERS
#define THOUSAND_ANT_HELPERS

#include "Transforms.hlsl"

VertexPositionInputs GetVertexPositionInputs(float4x4 m, float3 positionOS) 
{
    VertexPositionInputs inputs;

    //  TODO: Implement the Transform functions
    inputs.positionWS = TransformObjectToWorld(m, positionOS);
    inputs.positionVS = TransformWorldToView(inputs.positionWS);
    inputs.positionCS = TransformWorldToHClip(inputs.positionWS);

    float4 ndc = inputs.positionCS * 0.5f;
    inputs.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    inputs.positionNDC.zw = inputs.positionCS.zw;
    return inputs;
}

#endif
