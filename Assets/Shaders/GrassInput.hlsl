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
    half4 _WindSpeed;
    half _WaveSpeed;
    half _HeightCutoff;
CBUFFER_END

// ---------------------------------------------------------
// Grass Maps
// ---------------------------------------------------------
TEXTURE2D(_NoiseMap);        SAMPLER(sampler_NoiseMap);

#endif
