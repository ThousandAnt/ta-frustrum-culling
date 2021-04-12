float3 TransformObjectToWorld(float4x4 ltw, float3 positionOS)
{
    return mul(ltw, float4(positionOS, 1.0)).xyz;
}

float3 TransformObjectToWorldDir(float4x4 m, float3 dirOS, bool doNormalize = true) 
{
    float3 dirWS = mul((float3x3)m, dirOS);

    if (doNormalize) 
    {
        return SafeNormalize(dirWS);
    }

    return dirWS;
}
