float _NearDistance;
float _FarDistance;
float _NearAlpha;
float _FarAlpha;

float _remap(float value, float minSrc, float maxSrc, float minDst, float maxDst)
{
    if (value > maxSrc)
        return maxDst;
    if (value < minSrc)
        return minDst;
    return minDst + (value - minSrc) * (maxDst - minDst) / (maxSrc - minSrc);
}

float _distanceToCamera(float4 vertex)
{
    float3 worldPos = mul(unity_ObjectToWorld, vertex).xyz; // vertex pos in world.
    float3 viewPos = mul(unity_WorldToCamera, float4(worldPos, 1.0)).xyz; // vertex pos in camera.
    return length(viewPos); // distance from vertex to camera.
}

float DepthAlpha(float4 vertex)
{
    float distanceToCamera = _distanceToCamera(vertex);
    return _remap(distanceToCamera, _NearDistance, _FarDistance, _NearAlpha, _FarAlpha);
}
