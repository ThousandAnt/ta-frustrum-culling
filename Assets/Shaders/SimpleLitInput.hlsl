#ifndef THOUSAND_ANT_SIMPLE_INDIRECT_INPUT
#define THOUSAND_ANT_SIMPLE_INDIRECT_INPUT

#include "Specular.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
CBUFFER_END

#endif
