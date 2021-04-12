#ifndef THOUSAND_ANT_SIMPLE_LIT_PASS
#define THOUSAND_ANT_SIMPLE_LIT_PASS

#include "SimpleIndirectLitInput.hlsl"
#include "Helpers.hlsl"

// Interpolators IndirectLitPassVertex(MeshData input, uint instanceID : SV_INSTANCEID)
Interpolators IndirectLitPassVertex(MeshData input) // TODO: disable this
{
    // float4x4 ltw = _Matrices[instanceID]; // TODO: Regrab the 
    Interpolators output = (Interpolators)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float4x4 m = unity_ObjectToWorld;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(m, input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

    return output;
}

float4 IndirectLitPassFragment(Interpolators input) : SV_TARGET 
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

    half alpha = diffuseAlpha.a * _BaseColor.rgb;
    // clip(alpha - _Cutoff);
    AlphaDiscard(alpha, _Cutoff);

    // TODO: Add lighting
    return diffuseAlpha;
}

#endif
