float3 NormalTangentToWorld(float3 normalTS, float3 normalWS, float4 tangentWS)
{
    float3x3 tangentToWorld = CreateTangentToWorld(normalWS, tangentWS.xyz, tangentWS.w);
    return TransformTangentToWorld(normalTS, tangentToWorld);
}

// Sample the height map, using mipmaps.
float SampleHeight(float2 uv)
{
    return SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv).r;
}

// From: https://iquilezles.org/articles/texture/
float4 getTexel(float2 p)
{
    float2 newUV = p * _HeightMap_Width + 0.5;

    float2 i = floor(newUV);
    float2 f = frac(newUV);
    newUV = i + f * f * (3.0f - 2.0f * f);

    newUV = (newUV - 0.5) / _HeightMap_Width;
    return SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, newUV);
}

// from http://www.java-gaming.org/index.php?topic=35123.0
float4 cubic(float v)
{
    float4 n = float4(1.0, 2.0, 3.0, 4.0) - v;
    float4 s = n * n * n;
    float x = s.x;
    float y = s.y - 4.0 * s.x;
    float z = s.z - 4.0 * s.y + 6.0 * s.x;
    float w = 6.0 - x - y - z;
    return float4(x, y, z, w) * (1.0 / 6.0);
}

float4 textureBicubic(SamplerState samplerTex, float2 texCoords)
{
    float2 texSize = float2(_HeightMap_Width, _HeightMap_Height);
    float2 invTexSize = 1.0 / texSize;

    texCoords = texCoords * texSize - 0.5;

    float2 fxy = frac(texCoords);
    texCoords -= fxy;

    float4 xcubic = cubic(fxy.x);
    float4 ycubic = cubic(fxy.y);

    float4 c = texCoords.xxyy + float2(-0.5, +1.5).xyxy;

    float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
    float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;

    offset *= invTexSize.xxyy;

    float4 sample0 = _HeightMap.Sample(samplerTex, offset.xz);
    float4 sample1 = _HeightMap.Sample(samplerTex, offset.yz);
    float4 sample2 = _HeightMap.Sample(samplerTex, offset.xw);
    float4 sample3 = _HeightMap.Sample(samplerTex, offset.yw);

    float sx = s.x / (s.x + s.y);
    float sy = s.z / (s.z + s.w);

    return lerp(
        lerp(sample3, sample2, sx),
        lerp(sample1, sample0, sx),
        sy
    );
}

float3 GenerateNormalFromHeightMap(float2 uv)
{
    float2 uvIncrement = _HeightMap_TexelSize * 2.0f;
    // float2 uvIncrement = float2(0.01, 0.01);
    // Sample the height from adjacent pixels.
    float left = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(-1, 0) * uvIncrement.x).r;
    float right = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(1, 0) * uvIncrement.x).r;
    float down = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(0, -1) * uvIncrement.y).r;
    float up = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(0, 1) * uvIncrement.y).r;

    // float left = textureBicubic(sampler_HeightMap, uv + float2(-1, 0) * uvIncrement.x).r * _HeightMapAltitude;
    // float right = textureBicubic(sampler_HeightMap, uv + float2(1, 0) * uvIncrement.x).r * _HeightMapAltitude;
    // float down = textureBicubic(sampler_HeightMap, uv + float2(0, -1) * uvIncrement.x).r * _HeightMapAltitude;
    // float up = textureBicubic(sampler_HeightMap, uv + float2(0, 1) * uvIncrement.x).r * _HeightMapAltitude;

    // float left = getTexel(uv + float2(-1, 0) * uvIncrement.x).r * _HeightMapAltitude;
    // float right = getTexel(uv + float2(1, 0) * uvIncrement.x).r * _HeightMapAltitude;
    // float down = getTexel(uv + float2(a0, -1) * uvIncrement.y).r * _HeightMapAltitude;
    // float up = getTexel(uv + float2(0, 1) * uvIncrement.y).r * _HeightMapAltitude;

    // Generate a tangent space normal using the slope along the U and V axis.
    float3 normalTS = float3(
        (left - right) / (uvIncrement.x),
        (down - up) / (uvIncrement.y),
        1
    );

    normalTS.xy *= _SimulationNormalStrength; // Adjust the XY channels to create stronger or weaker normals.
    return normalize(normalTS);
}

// Bottom 5 wave functions from: https://jayconrod.com/posts/34/water-simulation-in-glsl 
float wave(float2 position, float2 direction, float amplitude, float wavelength, float speed, float time)
{
    float frequency = TWO_PI / wavelength;
    float phase = speed * frequency;
    float theta = dot(direction, position);
    return amplitude * sin(frequency * (theta + time * phase));
}

float waveHeight(float2 position)
{
    float height = 0.0;
    height += wave(position, float2(1, 0), 0.01f, 4.0f, 1, _Time.x);
    height += wave(position, float2(0, 1), 0.01f, 2.0f, 1, _Time.x);

    return height;
}

float dWavedx(float2 position, float2 direction, float amplitude, float wavelength, float speed, float time)
{
    float frequency = TWO_PI / wavelength;
    float phase = speed * frequency;
    float theta = dot(direction, position);
    float A = amplitude * direction.x * frequency;
    return A * cos(theta * frequency + time * phase);
}

float dWavedy(float2 position, float2 direction, float amplitude, float wavelength, float speed, float time)
{
    float frequency = TWO_PI / wavelength;
    float phase = speed * frequency;
    float theta = dot(direction, position);
    float A = amplitude * direction.y * frequency;
    return A * cos(theta * frequency + time * phase);
}

float3 waveNormal(float2 position)
{
    float dx = 0.0;
    float dy = 0.0;
    dx += dWavedx(position, float2(1, 0), 0.05f, 1.0f, 1, _Time.x);
    dy += dWavedy(position, float2(1, 0), 0.05f, 1.0f, 1, _Time.x);

    // dx += dWavedx(position, float2(0, 1), 0.05f, 1.0f, 1, _Time.x);
    // dy += dWavedy(position, float2(0, 1), 0.05f, 1.0f, 1, _Time.x);

    float3 n = float3(-dx, 1.0, -dy);
    return normalize(n);
}

float3 FlowUVW(float2 uv, float2 flowVector, float2 jump, float flowOffset, float tiling, float time, bool flowB)
{
    float phaseOffset = flowB ? 0.5 : 0;
    float progress = frac(time + phaseOffset);
    float3 uvw;

    uvw.xy = uv - flowVector * (progress + flowOffset);
    // uvw.xy *= tiling;
    uvw.xy += phaseOffset;
    uvw.xy += (time - progress) * jump;
    uvw.z = 1 - abs(1 - 2 * progress);
    return uvw;
}
