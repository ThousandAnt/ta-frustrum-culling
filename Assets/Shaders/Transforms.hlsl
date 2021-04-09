inline float3 TransformObjectToWorld(float4x4 ltw, float3 positionOS)
{
    return mul(ltw, float4(positionOS, 1.0)).xyz;
}
