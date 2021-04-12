#ifndef THOUSAND_ANT_SIMPLE_LIT_PASS
#define THOUSAND_ANT_SIMPLE_LIT_PASS

#include "SimpleIndirectLitInput.hlsl"
#include "Helpers.hlsl"

// Interpolators IndirectLitPassVertex(MeshData input, uint instanceID : SV_INSTANCEID)
Interpolators IndirectLitPassVertex(MeshData input) // TODO: disable this
{
    // float4x4 ltw = _Matrices[instanceID]; // TODO: Regrab the 
    Interpolators o = (Interpolators)0;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    return o;
}

float4 IndirectLitPassFragment(Interpolators input) : SV_TARGET 
{
    return float4(1, 0, 0, 1);
}

#endif
