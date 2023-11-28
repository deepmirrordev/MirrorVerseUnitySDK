// From https://github.com/miguel12345/UnityWireframeRenderer

float4 WireLineVert(float4 vertex)
{
    float4 outV = UnityObjectToClipPos(vertex);
    //If we are rendering in shaded mode (showing the original mesh renderer)
    //we want to ensure that the wireframe-processed mesh appears "on top" of
    //the original mesh. We achieve this by slightly decreasing the z component
    //(making the vertex closer to the camera) without actually changing its screen space position
    //since the w component remains the same, and thus, after w division, the x and y components
    //won't be affected by our "trick".
    //So, in essence, this just changes the value that gets written to the Z-Buffer
    outV.z -= 0.001;
    return outV;
}

float _DistanceSq(float2 pt1, float2 pt2)
{
    float2 v = pt2 - pt1;
    return dot(v, v);
}

float _MinimumDistance(float2 v, float2 w, float2 p)
{
  // Return minimum distance between line segment vw and point p
    float l2 = _DistanceSq(v, w); // i.e. |w-v|^2 -  avoid a sqrt
  // Consider the line extending the segment, parameterized as v + t (w - v).
  // We find projection of point p onto the line. 
  // It falls where t = [(p-v) . (w-v)] / |w-v|^2
  // We clamp t from [0,1] to handle points outside the segment vw.
    float t = max(0, min(1, dot(p - v, w - v) / l2));
    float2 projection = v + t * (w - v); // Projection falls on the segment
    return distance(p, projection);
}


float _CloestNormalizedDistance(float2 uv, float lineWidthInPixels)
{
    float2 uVector = float2(ddx(uv.x), ddy(uv.x)); //also known as tangent vector
    float2 vVector = float2(ddx(uv.y), ddy(uv.y)); //also known as binormal vector

    float vLength = length(uVector);
    float uLength = length(vVector);
    float uvDiagonalLength = length(uVector + vVector);

    float maximumUDistance = lineWidthInPixels * vLength;
    float maximumVDistance = lineWidthInPixels * uLength;
    float maximumUVDiagonalDistance = lineWidthInPixels * uvDiagonalLength;

    float leftEdgeUDistance = uv.x;
    float rightEdgeUDistance = (1.0 - leftEdgeUDistance);

    float bottomEdgeVDistance = uv.y;
    float topEdgeVDistance = 1.0 - bottomEdgeVDistance;

    float minimumUDistance = min(leftEdgeUDistance, rightEdgeUDistance);
    float minimumVDistance = min(bottomEdgeVDistance, topEdgeVDistance);
    float uvDiagonalDistance = _MinimumDistance(float2(0.0, 1.0), float2(1.0, 0.0), uv);

    float normalizedUDistance = minimumUDistance / maximumUDistance;
    float normalizedVDistance = minimumVDistance / maximumVDistance;
    float normalizedUVDiagonalDistance = uvDiagonalDistance / maximumUVDiagonalDistance;


    float closestNormalizedDistance = min(normalizedUDistance, normalizedVDistance);
    closestNormalizedDistance = min(closestNormalizedDistance, normalizedUVDiagonalDistance);
    return closestNormalizedDistance;
}

float WireLineAlpha(float2 uv, float lineWidthInPixels)
{
    float lineAntiaAliasWidthInPixels = 1;
    float closestNormalizedDistance = _CloestNormalizedDistance(uv, lineWidthInPixels);
    float lineAlpha = 1.0 - smoothstep(1.0, 1.0 + (lineAntiaAliasWidthInPixels / lineWidthInPixels), closestNormalizedDistance);
    return lineAlpha;
}
