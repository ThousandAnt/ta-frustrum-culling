#ifndef THOUSAND_ANT_SIMPLE_GRASS_LIT_PASS
#define THOUSAND_ANT_SIMPLE_GRASS_LIT_PASS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Helpers.hlsl"
#include "IndirectInput.hlsl"

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
    float2 sampledPos              : TEXCOORD8;
};

void InitializeInputData(Interpolators input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWS;
#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
    half3 viewDirWS = input.viewDirWS;
    inputData.normalWS = input.normal;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;

    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

Interpolators IndirectLitPassVertex(MeshData input, uint instanceID : SV_INSTANCEID)
{
    Interpolators output = (Interpolators)0;
    float4x4 m = _Matrices[instanceID];

    float3 inputPos = input.positionOS.xyz;
    float normalizedHeight = saturate(inputPos.y / _HeightCutoff);

    float2 samplePos = TransformObjectToWorld(m, inputPos).xz / _WorldSize;
    samplePos += _Time.x * _WindSpeed.zw;
    float windSample = tex2Dlod(sampler_NoiseMap, float4(samplePos, 0, 0));

    inputPos.x += cos(_Time.x * _WaveSpeed * windSample) * _WindSpeed.x * normalizedHeight;
    inputPos.z += sin(_Time.x * _WaveSpeed * windSample) * _WindSpeed.y * normalizedHeight;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(m, inputPos);
    VertexNormalInputs normalInput   = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDirWS = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    return output;
}

float4 IndirectLitPassFragment(Interpolators input) : SV_TARGET 
{
    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    float3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

    float alpha = diffuseAlpha.a * _BaseColor.a;
    AlphaDiscard(alpha, _Cutoff);

    float2 samplePos = input.posWS.xz / _WorldSize;
    samplePos += _Time.x * _WindSpeed.zw;

    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_SpecGlossMap, sampler_EmissionMap));
    half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    half smoothness = specular.a;

    InputData inputData;
    InitializeInputData(input, normalTS, inputData);

    half4 color = UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);

    return color;
}

#endif
