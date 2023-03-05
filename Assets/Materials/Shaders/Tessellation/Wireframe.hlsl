#if !defined(FLAT_WIREFRAME_INCLUDED)
#define FLAT_WIREFRAME_INCLUDED

#pragma geometry GeometryProgram

[maxvertexcount(3)]
void GeometryProgram(triangle Interpolators input[3], inout TriangleStream<Interpolators> stream)
{
    // Flat shading
    // float3 p0 = input[0].positionWS.xyz;
    // float3 p1 = input[1].positionWS.xyz;
    // float3 p2 = input[2].positionWS.xyz;

    // float3 triangleNormal = normalize(cross(p1 - p0, p2 - p0));
    // input[0].normalWS = triangleNormal;
    // input[1].normalWS = triangleNormal;
    // input[2].normalWS = triangleNormal;

    input[0].barycentricCoordinates = float2(1, 0);
    input[1].barycentricCoordinates = float2(0, 1);
    input[2].barycentricCoordinates = float2(0, 0);

    stream.Append(input[0]);
    stream.Append(input[1]);
    stream.Append(input[2]);
}

#define APPLY_WIREFRAME(color) \
    input.barycentricCoordinates.z = 1 - input.barycentricCoordinates.x - input.barycentricCoordinates.y; \
    float3 deltas = fwidth(input.barycentricCoordinates); \
    input.barycentricCoordinates = smoothstep(deltas, 2 * deltas, input.barycentricCoordinates); \
    float minBary = min(input.barycentricCoordinates.x, min(input.barycentricCoordinates.y, input.barycentricCoordinates.z)); \
    color *= minBary

#endif
