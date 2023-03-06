#if !defined(TESSELLATION_INCLUDED)
#define TESSELLATION_INCLUDED

#pragma hull Hull
#pragma domain Domain

#define CONTROL_POINTS 3
#define BEZIER_FACTORS_CONTROL_POINTS CONTROL_POINTS
#define INPUT_TYPE "tri"

struct Attributes
{
    float3 vertexOS : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct TessellationControlPoint
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : INTERNALTESSPOS; // POSITION semantic is forbidden in this structure, so use INTERNALTESSPOS instead.
    float3 normalWS : NORMAL;
    float2 uv : TEXCOORD0;
    float2 uv_MainTex : TEXCOORD1;
    float3 positionOAS : TEXCOORD2;
    float3 normalOS : TEXCOORD3;
    float4 tangentWS : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct TessellationFactors
{
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

struct Interpolators
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : INTERNALTESSPOS;
    float3 normalWS : NORMAL;
    float2 uv : TEXCOORD0;
    float2 uv_MainTex : TEXCOORD1;
    float3 positionOAS : TEXCOORD2;
    float3 normalOS : TEXCOORD3;
    float4 tangentWS : TEXCOORD4;
    float fogCoords : TEXCOORD5;
    float2 barycentricCoordinates : TEXCOORD6;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

[domain(INPUT_TYPE)] // Signal we're inputting triangles.
[outputcontrolpoints(CONTROL_POINTS)] // Output patch type. Triangles have 3 points.
[outputtopology("triangle_cw")] // Signal we're outputting triangles.
[patchconstantfunc("PatchConstantFunction")] // Register the patch constant function.
// Select a partitioning algorithm for the tessellator to use to subdivide the patch: integer, fractional_odd, fractional_even, or pow2.
// Select a partitioning mode based on keywords
#if defined(_PARTITIONING_INTEGER)
[partitioning("integer")]
#elif defined(_PARTITIONING_FRAC_EVEN)
    [partitioning("fractional_even")]
#elif defined(_PARTITIONING_FRAC_ODD)
    [partitioning("fractional_odd")]
#elif defined(_PARTITIONING_POW2)
    [partitioning("pow2")]
#else
    [partitioning("fractional_odd")]
#endif
// Hull function runs once per vertex.
TessellationControlPoint Hull(InputPatch<TessellationControlPoint, CONTROL_POINTS> patch, uint id : SV_OutputControlPointID)
{
    return patch[id];
}

// Returns true is the point is outside the bounds set by lower and higher.
bool IsOutOfBounds(float3 p, float3 lower, float3 higher)
{
    return p.x < lower.x || p.x > higher.x || p.y < lower.y || p.y > higher.y || p.z < lower.z || p.z > higher.z;
}

// Return true if the given vertex is outside the camera frustum and should be culled.
bool IsPointOutOfFrustum(float4 positionCS, float tolerance)
{
    float3 culling = positionCS.xyz;
    float w = positionCS.w;

    // UNITY_RAW_FAR_CLIP_VALUE is either 0 or 1, depending on the Graphics API.
    // Most use 0, however OpenGL uses 1.
    float3 lowerBounds = float3(-w - tolerance, -w - tolerance, -w * UNITY_RAW_FAR_CLIP_VALUE - tolerance);
    float3 higherBounds = float3(w + tolerance, w + tolerance, w + tolerance);
    return IsOutOfBounds(culling, lowerBounds, higherBounds);
}

// Return true if the points in this triangle are wound counter-clockwise.
bool ShouldBackFaceCull(float4 p0PositionCS, float4 p1PositionCS, float4 p2PositionCS, float tolerance)
{
    float3 point0 = p0PositionCS.xyz / p0PositionCS.w;
    float3 point1 = p1PositionCS.xyz / p1PositionCS.w;
    float3 point2 = p2PositionCS.xyz / p2PositionCS.w;

    // In clip space, the view direction is float3(0, 0, 1), so we can just test the z coord.
    #if UNITY_REVERSED_Z
    return normalize(cross(point1 - point0, point2 - point0).z) < -tolerance;
    #else // In OpenGL, the test is reversed.
        return normalize(cross(point1 - point0, point2 - point0).z) > tolerance;
    #endif
}

// Returns true if it should be clipped due to frustum or winding culling.
bool ShouldClipPatch(float4 p0PositionCS, float4 p1PositionCS, float4 p2PositionCS, float frustumTolerance, float windingTolerance)
{
    bool allOutside = IsPointOutOfFrustum(p0PositionCS, frustumTolerance) &&
        IsPointOutOfFrustum(p1PositionCS, frustumTolerance) &&
        IsPointOutOfFrustum(p2PositionCS, frustumTolerance);
    return allOutside || ShouldBackFaceCull(p0PositionCS, p1PositionCS, p2PositionCS, windingTolerance);
}

// Calculate the tessellation factor for an edge.
// This function needs the world and clip space positions of the connected vertices
float EdgeTessellationFactor(float scale, float bias, float multiplier, float3 p0PositionWS, float4 p0PositionCS, float3 p1PositionWS, float4 p1PositionCS)
{
    float factor = 1;

    #if defined(_TESSELLATION_FACTOR_CONSTANT)
    factor = scale;
    #elif defined(_TESSELLATION_FACTOR_WORLD)
        factor = distance(p0PositionWS, p1PositionWS) / scale;
    #elif defined(_TESSELLATION_FACTOR_WORLD_WITH_DEPTH)
        float length = distance(p0PositionWS, p1PositionWS);
        float distanceToCamera = distance(GetCameraPositionWS(), (p0PositionWS + p1PositionWS) * 0.5);
        factor = length / (scale * distanceToCamera * distanceToCamera);
    #elif defined(_TESSELLATION_FACTOR_SCREEN)
        factor = distance(p0PositionCS.xyz / p0PositionCS.w, p1PositionCS.xyz / p1PositionCS.w) * _ScreenParams.y / scale;
    #endif

    return max(1, (factor + bias) * multiplier);
}

// The patch constant function runs once per patch and in parallel to the hull function.
TessellationFactors PatchConstantFunction(InputPatch<TessellationControlPoint, CONTROL_POINTS> patch)
{
    UNITY_SETUP_INSTANCE_ID(patch[0]); // Setup instancing.

    TessellationFactors factors = (TessellationFactors)0;
    // Check if this patch should be culled (it is out of view)
    if (ShouldClipPatch(patch[0].positionCS, patch[1].positionCS, patch[2].positionCS, _FrustumCullTolerance, _BackFaceCullTolerance))
    {
        factors.edge[0] = factors.edge[1] = factors.edge[2] = factors.inside = 0; // Cull the patch.
    }
    else
    {
        // Calculate tessellation factors
        factors.edge[0] = EdgeTessellationFactor(_TessellationFactor, _TessellationBias, 1, patch[1].positionWS, patch[1].positionCS, patch[2].positionWS,
                                                 patch[2].positionCS);
        factors.edge[1] = EdgeTessellationFactor(_TessellationFactor, _TessellationBias, 1, patch[2].positionWS, patch[2].positionCS, patch[0].positionWS,
                                                 patch[0].positionCS);
        factors.edge[2] = EdgeTessellationFactor(_TessellationFactor, _TessellationBias, 1, patch[0].positionWS, patch[0].positionCS, patch[1].positionWS,
                                                 patch[1].positionCS);
        factors.inside = (factors.edge[0] + factors.edge[1] + factors.edge[2]) / 3.0;
        // factors.inside = (
        //     EdgeTessellationFactor(_TessellationFactor, _TessellationBias, 1, patch[1].positionWS, patch[1].positionCS, patch[2].positionWS,
        //                            patch[2].positionCS), EdgeTessellationFactor(_TessellationFactor, _TessellationBias, 1, patch[2].positionWS,
        //                                                                         patch[2].positionCS, patch[0].positionWS, patch[0].positionCS),
        //     EdgeTessellationFactor(_TessellationFactor, _TessellationBias, 1, patch[0].positionWS, patch[0].positionCS, patch[1].positionWS,
        //                            patch[1].positionCS)) / 3.0;
    }

    return factors;
}

// Barycentric interpolation as a function
float3 BarycentricInterpolate(float3 bary, float3 a, float3 b, float3 c)
{
    return bary.x * a + bary.y * b + bary.z * c;
}

#define BARYCENTRIC_INTERPOLATE(fieldName) output.fieldName = \
    patch[0].fieldName * barycentricCoordinates.x + \
    patch[1].fieldName * barycentricCoordinates.y + \
    patch[2].fieldName * barycentricCoordinates.z

// Calculate Phong projection offset
float3 PhongProjectedPosition(float3 flatPositionWS, float3 cornerPositionWS, float3 normalWS)
{
    return flatPositionWS - dot(flatPositionWS - cornerPositionWS, normalWS) * normalWS;
}

// Apply Phong smoothing
float3 CalculatePhongPosition(float3 bary, float smoothing, float3 p0PositionWS, float3 p0NormalWS, float3 p1PositionWS, float3 p1NormalWS, float3 p2PositionWS,
                              float3 p2NormalWS)
{
    float3 flatPositionWS = BarycentricInterpolate(bary, p0PositionWS, p1PositionWS, p2PositionWS);
    float3 smoothedPositionWS =
        bary.x * PhongProjectedPosition(flatPositionWS, p0PositionWS, p0NormalWS) +
        bary.y * PhongProjectedPosition(flatPositionWS, p1PositionWS, p1NormalWS) +
        bary.z * PhongProjectedPosition(flatPositionWS, p2PositionWS, p2NormalWS);
    return lerp(flatPositionWS, smoothedPositionWS, smoothing);
}

#endif
