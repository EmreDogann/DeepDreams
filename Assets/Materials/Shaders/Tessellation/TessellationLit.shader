Shader "Custom/TessellationLit"
{
    Properties
    {
        [Header(Textures)] [Space] [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _FogCube("Fog Cube Texture", CUBE) = "" {}
        [NoScaleOffset] _WaterNormal1("Water Normal Map 1", 2D) = "bump" {}
        [NoScaleOffset] _WaterNormal2("Water Normal Map 2", 2D) = "bump" {}
        _SimulationNormalStrength("Simulation Normal Strength", Float) = 1
        _NormalMapStrength("Water Normal Map Strength", Float) = 1
        _NormalMapSize("Water Normal Map Size", Float) = 1
        [NoScaleOffset] _WaterFlowMap("Water Flow Map (RG, A noise)", 2D) = "black" {}
        _Tiling ("Tiling", Float) = 1
        _Speed ("Speed", Float) = 1
        _FlowStrength ("Flow Strength", Float) = 1
        _FlowOffset ("Flow Offset", Float) = 0
        _UJump ("Flow Map U jump per phase", Range(-0.25, 0.25)) = 0.25
		_VJump ("Flow Map V jump per phase", Range(-0.25, 0.25)) = 0.25
        _Smoothness("Smoothness", Float) = 0
        
        [Header(Tessellation)] [Space] [NoScaleOffset] _HeightMap("Tessellation Height Map", 2D) = "black" {}
        _HeightMapAltitude("Height Map Altitude", Range(0, 100)) = 1
        // This keyword enum allows us to choose between partitioning modes. It's best to try them out for yourself
        [KeywordEnum(INTEGER, FRAC_EVEN, FRAC_ODD, POW2)] _PARTITIONING("Partition algoritm", Float) = 0
        // This allows us to choose between tessellation factor methods
        [KeywordEnum(CONSTANT, WORLD, SCREEN, WORLD_WITH_DEPTH)] _TESSELLATION_FACTOR("Tessellation mode", Float) = 0
        // This factor is applied differently per factor mode
        //  Constant: not used
        //  World: this is the ideal edge length in world units. The algorithm will try to keep all edges at this value
        //  Screen: this is the ideal edge length in screen pixels. The algorithm will try to keep all edges at this value
        //  World with depth: similar to world, except the edge length is decreased quadratically as the camera gets closer 
        _TessellationFactor("Tessellation factor", Range(0, 100)) = 1
        // This value is added to the tessellation factor. Use if your model should be more or less tessellated by default
        _TessellationBias("Tessellation bias", Range(0, 100)) = 0
        // A tolerance to frustum culling. Increase if triangles disappear when on screen
        _FrustumCullTolerance("Frustum cull tolerance", Range(0, 100)) = 0.01
        // A tolerance to back face culling. Increase if holes appear on your mesh
        _BackFaceCullTolerance("Back face cull tolerance", Range(0, 100)) = 0.01
        // This keyword selects a tessellation smoothing method
        //  Flat: no smoothing
        //  Phong: use Phong tessellation, as described here: http://www.klayge.org/material/4_0/PhongTess/PhongTessellation.pdf'
        [KeywordEnum(FLAT, PHONG, BEZIER_QUAD_NORMALS)] _TESSELLATION_SMOOTHING("Smoothing mode", Float) = 0
        // A factor to interpolate between flat and the selected smoothing method
        _TessellationSmoothing("Smoothing factor", Range(0, 1)) = 0.75
        
        [Header(Triplanar Mapping)] [Space] [Toggle(ENABLE_TRIPLANAR)] _Triplanar("Enable Triplanar Mapping", Float) = 0
        [Toggle(ENABLE_ALIGNED_TRIPLANAR)] _TriplanarAligned("Object Aligned Triplanar Mapping", Float) = 0
        _TriplanarScale("Triplanar Scale", Float) = 0
        _TriplanarBlendOffset("Blend Offset", Range(0, 0.5)) = 0.25
        _TriplanarBlendSharpness("Blend Sharpness", Range(1, 64)) = 1
        
        [Header(Wireframe)] [Space] [Toggle(ENABLE_WIREFRAME)] _Wireframe("Enable Wireframe", Float) = 0
        _WireframeColor ("Wireframe Color", Color) = (0, 0, 0, 1)
		_WireframeSmoothing ("Wireframe Smoothing", Range(0, 10)) = 1
		_WireframeThickness ("Wireframe Thickness", Range(0, 10)) = 1
    }
    
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "PreviewType" = "Plane"
            "ShaderModel" = "5.0"
        }
        
        Pass
        {
            Name "Tessellation"
            Tags {"LightMode" = "UniversalForward"}
            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask RGB
            
            HLSLPROGRAM
            #pragma target 5.0 // 5.0 required for tessellation.
            
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            // Material keywords - Lighting
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            // Material keywords - Tessellation
            #pragma shader_feature_local _PARTITIONING_INTEGER _PARTITIONING_FRAC_EVEN _PARTITIONING_FRAC_ODD _PARTITIONING_POW2
            #pragma shader_feature_local _TESSELLATION_SMOOTHING_FLAT _TESSELLATION_SMOOTHING_PHONG, _TESSELLATION_SMOOTHING_BEZIER_QUAD_NORMALS
            #pragma shader_feature_local _TESSELLATION_FACTOR_CONSTANT _TESSELLATION_FACTOR_WORLD _TESSELLATION_FACTOR_SCREEN _TESSELLATION_FACTOR_WORLD_WITH_DEPTH

            // Material keywords - Triplanar Mapping
            #pragma shader_feature_local ENABLE_TRIPLANAR
            #pragma shader_feature_local ENABLE_ALIGNED_TRIPLANAR
            #pragma shader_feature_local ENABLE_WIREFRAME

            // Material keywords - Lighting
            #define _SPECULAR_COLOR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Materials/Shaders/Common.hlsl"
            
            Texture2D _MainTex; SamplerState sampler_MainTex;
            
            Texture2D _WaterNormal1; SamplerState sampler_WaterNormal1;
            Texture2D _WaterNormal2; SamplerState sampler_WaterNormal2;
            Texture2D _WaterFlowMap; SamplerState sampler_WaterFlowMap;
            
            Texture2D _HeightMap; SamplerState sampler_HeightMap;
            TextureCube _FogCube; SamplerState sampler_FogCube; // Skybox cube texture

            // CBUFFER section needed for SRP batching.
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _Smoothness;
                float _UJump;
                float _VJump;
                float _Tiling;
                float _Speed;
                float _FlowStrength;
                float _FlowOffset;

                float _NormalMapStrength;
                float _NormalMapSize;
                float _SimulationNormalStrength;
                float4 _HeightMap_TexelSize;
                float _HeightMapAltitude;
            
                float3 _FactorEdge1;
                float _FactorInside;
                float _TessellationFactor;
                float _TessellationBias;
                float _TessellationSmoothing;
                float _FrustumCullTolerance;
                float _BackFaceCullTolerance;
            
                float _TriplanarScale;
                float _TriplanarBlendOffset;
                float _TriplanarBlendSharpness;

                float4 _WireframeColor;
                float _WireframeSmoothing;
                float _WireframeThickness;

                float _HeightMap_Width;
                float _HeightMap_Height;
            
                // Extract scale from object to world matrix.
                static float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
            CBUFFER_END
            
            #include "Tessellation.hlsl"
            // #include "Wireframe.hlsl"

            // float3 filterNormal(float2 uv, float texelSize, int terrainSize)
            // {
            //     float4 h;
            //     h[0] = tex2D(_HeightMap, uv + texelSize*float2(0,-1)).r * _HeightMapAltitude; down
            //     h[1] = tex2D(_HeightMap, uv + texelSize*float2(-1,0)).r * _HeightMapAltitude; left
            //     h[2] = tex2D(_HeightMap, uv + texelSize*float2(1,0)).r * _HeightMapAltitude; right
            //     h[3] = tex2D(_HeightMap, uv + texelSize*float2(0,1)).r * _HeightMapAltitude; up
            //
            //     float3 n;
            //     n.z = -(h[0] - h[3]);
            //     n.x = (h[1] - h[2]);
            //     n.y = 2 * texelSize * terrainSize; // pixel space -> uv space -> world space
            //
            //     return normalize(n);
            // }

            // Sample the height map, using mipmaps.
            float SampleHeight(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv).r;
            }
            
            // Calculate a normal vector by sampling the height map.
            // float3 GenerateNormalFromHeightMap(float2 uv)
            // {
            //     float2 uvIncrement = _HeightMap_TexelSize;
            //     // float2 uvIncrement = float2(0.01, 0.01);
            //     // Sample the height from adjacent pixels.
            //     float left = SampleHeight(uv - float2(uvIncrement.x, 0));
            //     float right = SampleHeight(uv + float2(uvIncrement.x, 0));
            //     float down = SampleHeight(uv - float2(0, uvIncrement.y));
            //     float up = SampleHeight(uv + float2(0, uvIncrement.y));
            //
            //     // Generate a tangent space normal using the slope along the U and V axis.
            //     float3 normalTS = float3(
            //         (left - right) / (uvIncrement.x * 2),
            //         (down - up) / (uvIncrement.y * 2),
            //         1
            //     );
            //
            //     normalTS.xy *= _NormalStrength; // Adjust the XY channels to create stronger or weaker normals.
            //     return normalize(normalTS);
            // }

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
            float4 cubic(float v){
                float4 n = float4(1.0, 2.0, 3.0, 4.0) - v;
                float4 s = n * n * n;
                float x = s.x;
                float y = s.y - 4.0 * s.x;
                float z = s.z - 4.0 * s.y + 6.0 * s.x;
                float w = 6.0 - x - y - z;
                return float4(x, y, z, w) * (1.0/6.0);
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

                float4 c = texCoords.xxyy + float2 (-0.5, +1.5).xyxy;
                
                float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
                float4 offset = c + float4 (xcubic.yw, ycubic.yw) / s;
                
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

                // float original = getTexel(uv).r;
                // // float left = getTexel(uv + float2(-1, 0) * uvIncrement.x).r;
                // float right = getTexel(uv + float2(1, 0) * uvIncrement.x).r;
                // // float down = getTexel(uv + float2(0, -1) * uvIncrement.y).r;
                // float up = getTexel(uv + float2(0, 1) * uvIncrement.y).r;
                //
                // // float3 va = normalize(float3(1.0 * uvIncrement.x, left - right, 0.0f));
                // // float3 vb = normalize(float3(0.0, up - down, 1.0 * uvIncrement.y));
                // float3 va = float3(1.0 * uvIncrement.x, 0.0f, right - original);
                // float3 vb = float3(0.0, 1.0 * uvIncrement.y, up - original);
                // return normalize(cross(va, vb) * _NormalStrength);

                // Sample the height from adjacent pixels.
                // float left = getTexel(uv + float2(-1, 0) * uvIncrement.x).g;
                // float right = getTexel(uv + float2(1, 0) * uvIncrement.x).g;
                // float down = getTexel(uv + float2(0, -1) * uvIncrement.y).g;
                // float up = getTexel(uv + float2(0, 1) * uvIncrement.y).g;

                // float left = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uv + float2(-1, 0) * uvIncrement.x, 0).r * _HeightMapAltitude;
                // float right = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap,uv + float2(1, 0) * uvIncrement.x, 0).r * _HeightMapAltitude;
                // float down = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap,uv + float2(0, -1) * uvIncrement.y, 0).r * _HeightMapAltitude;
                // float up = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap,uv + float2(0, 1) * uvIncrement.y, 0).r * _HeightMapAltitude;
                
                // float3 normalTS = float3(left - right, down - up, 1.0f);
                
                // Generate a tangent space normal using the slope along the U and V axis.
                // float3 normalTS = float3(
                //     (left - right) / (uvIncrement.x * 2),
                //     (down - up) / (uvIncrement.y * 2),
                //     1
                // );
                //
                // normalTS.xy *= _NormalStrength; // Adjust the XY channels to create stronger or weaker normals.
                // return normalize(normalTS);
            }

            // float wave(float2 position, float2 direction, float amplitude, float wavelength, float speed, float time) {
            //     float frequency = TWO_PI / wavelength;
            //     float phase = speed * frequency;
            //     float theta = dot(direction, position);
            //     return amplitude * sin(theta * frequency + time * phase);
            // }
            
            float wave(float2 position, float2 direction, float amplitude, float wavelength, float speed, float time) {
                float frequency = TWO_PI / wavelength;
                float phase = speed * frequency;
                float theta = dot(direction, position);
                return amplitude * sin(frequency * (theta - time * phase));
            }

            float waveHeight(float2 position)
            {
                float height = 0.0;
                height += wave(position, float2(1, 0), 0.01f, 4.0f, 1, _Time.x);
                height += wave(position, float2(0, 1), 0.01f, 2.0f, 1, _Time.x);
                
                return height;
            }

            float dWavedx(float2 position, float2 direction, float amplitude, float wavelength, float speed, float time) {
                float frequency = TWO_PI / wavelength;
                float phase = speed * frequency;
                float theta = dot(direction, position);
                float A = amplitude * direction.x * frequency;
                return A * cos(theta * frequency + time * phase);
            }
            
            float dWavedy(float2 position, float2 direction, float amplitude, float wavelength, float speed, float time) {
                float frequency = TWO_PI / wavelength;
                float phase = speed * frequency;
                float theta = dot(direction, position);
                float A = amplitude * direction.y * frequency;
                return A * cos(theta * frequency + time * phase);
            }

            float3 waveNormal(float2 position) {
                float dx = 0.0;
                float dy = 0.0;
                dx += dWavedx(position, float2(1, 0), 0.05f, 1.0f, 1, _Time.x);
                dy += dWavedy(position, float2(1, 0), 0.05f, 1.0f, 1, _Time.x);
                
                // dx += dWavedx(position, float2(0, 1), 0.05f, 1.0f, 1, _Time.x);
                // dy += dWavedy(position, float2(0, 1), 0.05f, 1.0f, 1, _Time.x);
                
                float3 n = float3(-dx, 1.0, -dy);
                return normalize(n);
            }

            TessellationControlPoint Vertex(Attributes input)
            {
                TessellationControlPoint output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.vertexOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                // OAS - Object Aligned Space
                // Apply only the scale to the object space vertex in order to compensate for rotation.
                output.positionOAS = input.vertexOS.xyz;
                output.normalOS = input.normalOS.xyz;

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w); // tangent.w contains bitangent multiplier;
                output.uv = input.uv;
                output.uv_MainTex = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }
            
            // The domain function runs once per vertex in the final, tessellated mesh.
            // Use it to reposition vertices and prepare for the fragment stage.
            [domain(INPUT_TYPE)] // Signal we're inputting triangles.
            Interpolators Domain(TessellationFactors factors, OutputPatch<TessellationControlPoint, CONTROL_POINTS> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                // Setup instancing and stereo support (for VR)
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                Interpolators output = (Interpolators)0;
                // Calculate smoothed position, normal, and tangent
                // This rounds a triangle to smooth model silhouettes and improve normal interpolation
                // It can use either flat (no smoothing), Phong, or bezier-based smoothing, depending on material settings
                #if defined(_TESSELLATION_SMOOTHING_PHONG)
                    output.positionWS = CalculatePhongPosition(barycentricCoordinates, _TessellationSmoothing,
                        patch[0].positionWS, patch[0].normalWS, patch[1].positionWS, patch[1].normalWS, patch[2].positionWS, patch[2].normalWS);
                #elif defined(_TESSELLATION_SMOOTHING_BEZIER_LINEAR_NORMALS) || defined(_TESSELLATION_SMOOTHING_BEZIER_QUAD_NORMALS)
                    output.positionWS = CalculateBezierPosition(barycentricCoordinates, _TessellationSmoothing, factors.bezierPoints, patch[0].positionWS, patch[1].positionWS, patch[2].positionWS);
                #else
                    BARYCENTRIC_INTERPOLATE(positionWS);
                #endif

                #if defined(_TESSELLATION_SMOOTHING_BEZIER_QUAD_NORMALS)
                    float3 normalWS, tangentWS;
                    CalculateBezierNormalAndTangent(barycentricCoordinates, _TessellationSmoothing, factors.bezierPoints,
                        patch[0].normalWS, patch[0].tangentWS.xyz, patch[1].normalWS, patch[1].tangentWS.xyz, patch[2].normalWS, patch[2].tangentWS.xyz,
                        normalWS, tangentWS);
                #else
                    BARYCENTRIC_INTERPOLATE(normalWS);
                    BARYCENTRIC_INTERPOLATE(tangentWS.xyz);
                    float3 normalWS = normalize(output.normalWS);
                    float3 tangentWS = normalize(output.tangentWS.xyz);
                #endif
                
                BARYCENTRIC_INTERPOLATE(positionOAS);
                BARYCENTRIC_INTERPOLATE(normalOS);
                BARYCENTRIC_INTERPOLATE(uv);
                BARYCENTRIC_INTERPOLATE(uv_MainTex);
                
                // noise.octaves = 1;
                // noise.frequency = 1.0f;
                // output.positionWS += normalWS * fnlGetNoise2D(noise, output.positionWS.x, output.positionWS.z  + _Time.y / 5) / 8;
                // noise.octaves = 5;
                // noise.frequency = 0.5f;
                // output.positionWS += normalWS * fnlGetNoise2D(noise, output.positionWS.x + _Time.y / 5, output.positionWS.z) / 8;
                // output.normalWS = waveNormal(output.positionWS.xz);
                
                // Apply height map.
                const float height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, output.uv, 0).r * _HeightMapAltitude;
                output.positionWS += output.normalWS * height;
                output.positionWS += waveHeight(output.positionWS.xz);
                
                output.normalWS = normalWS;
                output.tangentWS = float4(tangentWS, patch[0].tangentWS.w);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.positionOAS *= scale; // OAS = Object Aligned Space. Apply only the scale to the object space vertex in order to compensate for rotation.
                output.fogCoords = ComputeFogFactor(output.positionCS.z);
                output.viewDir = GetWorldSpaceNormalizeViewDir(output.positionWS);
                
                return output;
            }

            float3 FlowUVW(float2 uv, float2 flowVector, float2 jump, float flowOffset, float tiling, float time, bool flowB) {
                float phaseOffset = flowB ? 0.5 : 0;
                float progress = frac(time + phaseOffset);
                float3 uvw;
                
                uvw.xy = uv - flowVector * (progress + flowOffset);
                uvw.xy *= tiling;
                uvw.xy += phaseOffset;
                uvw.xy += (time - progress) * jump;
                uvw.z = 1 - abs(1 - 2 * progress);
	            return uvw;
            }
            
            float4 Fragment(Interpolators input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                #if defined(ENABLE_TRIPLANAR)
                    // References: https://www.patreon.com/posts/quick-game-art-16714688
                    // https://catlikecoding.com/unity/tutorials/advanced-rendering/triplanar-mapping/
                    // https://bgolus.medium.com/normal-mapping-for-a-triplanar-shader-10bf39dca05a#1997
                    // https://forum.unity.com/threads/box-triplanar-mapping-following-object-rotation.501252/
                    #if defined(ENABLE_ALIGNED_TRIPLANAR)
                        float3 uvScaled = input.positionOAS * _TriplanarScale;
                        float3 blending = abs(input.normalOS);
                        half3 axisSign = sign(input.normalOS); // Get the sign (-1 or 1) of the surface normal.
                    #else
                        float3 uvScaled = input.positionWS * _TriplanarScale;
                        float3 blending = abs(input.normalWS);
                        half3 axisSign = sign(input.normalWS); // Get the sign (-1 or 1) of the surface normal.
                    #endif
                    
                    // Triplanar uvs
                    float2 uvX = uvScaled.yz; // x facing plane
                    float2 uvY = uvScaled.xz; // y facing plane
                    float2 uvZ = uvScaled.xy; // z facing plane

                    // Flip UVs to correct for mirroring
                    uvX.x *= axisSign.x;
                    uvY.x *= axisSign.y;
                    uvZ.x *= -axisSign.z;
                
                    blending = saturate(blending - _TriplanarBlendOffset);
                    blending = pow(blending, _TriplanarBlendSharpness);
                    blending /= dot(blending, float3(1,1,1));

                    float4 color = blending.z * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvZ);
                    color += blending.x * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvX);
                    color += blending.y * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvY);
                #else
                    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv_MainTex);
                #endif
                color *= _BaseColor;

                
                // Fog Reference: https://github.com/keijiro/KinoFog/blob/master/Assets/Kino/Fog/Shader/Fog.shader
                // https://www.reddit.com/r/Unity3D/comments/7wm02n/perfect_fog_for_your_game_trouble_matching_fog/
                // float4 fogCube = SAMPLE_TEXTURECUBE(_FogCube, sampler_FogCube, -viewDir);
                
                // color = float4(MixFogColor(color.rgb, fogCube.rgb, input.fogCoords), color.a);

                #if defined(ENABLE_WIREFRAME)
                    // For Wireframe.
                    float3 barys;
	                barys.xy = input.barycentricCoordinates;
                    barys.z = 1 - barys.x - barys.y;
	                float3 deltas = fwidth(barys); // Keep the line width constant in screen space using screen-space derivative.
                    float3 smoothing = deltas * _WireframeSmoothing;
	                float3 thickness = deltas * _WireframeThickness;
                    
                    barys = smoothstep(thickness, thickness + smoothing, barys);
                    float minBary = min(barys.x, min(barys.y, barys.z));
                    color = lerp(_WireframeColor, color, minBary);
                #endif
                
                float3 flow = SAMPLE_TEXTURE2D(_WaterFlowMap, sampler_WaterFlowMap, input.uv).rgb;
			    flow.xy = flow.xy * 2 - 1; // Map is in 0-1 range. We have to map it back to -1-1 range using *2-1.
                flow *= _FlowStrength;
                float noise = SAMPLE_TEXTURE2D(_WaterFlowMap, sampler_WaterFlowMap, input.uv).a; // Greyscale noise in alpha channel.
			    float time = _Time.x * _Speed + noise;
                float2 jump = float2(_UJump, _VJump);
                float3 flowUVW_A = FlowUVW(input.uv, flow.xy, jump, _FlowOffset, _Tiling, time, false);
                float3 flowUVW_B = FlowUVW(input.uv, flow.xy, jump, _FlowOffset, _Tiling, time, true);
                
                float3 waterNormal1_A = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal1, sampler_WaterNormal1, flowUVW_A.xy * _NormalMapSize), _NormalMapStrength) * flowUVW_A.z;
                float3 waterNormal2_A = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal2, sampler_WaterNormal2, flowUVW_A.xy * _NormalMapSize - time / 2), _NormalMapStrength) * flowUVW_A.z;
                
                float3 waterNormal1_B = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal1, sampler_WaterNormal1, flowUVW_B.xy * _NormalMapSize), _NormalMapStrength) * flowUVW_B.z;
                float3 waterNormal2_B = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal2, sampler_WaterNormal2, flowUVW_B.xy * _NormalMapSize - time / 2), _NormalMapStrength) * flowUVW_B.z;
                // float3 normalTS = BlendNormal(BlendNormal(waterNormal1_A, waterNormal1_B), BlendNormal(waterNormal2_A, waterNormal2_B)) * flow.z;
                float3 normalTS = ((waterNormal1_A + waterNormal1_B) + (waterNormal2_A + waterNormal2_B)) * flow.z;
                normalTS = BlendNormal(normalTS, GenerateNormalFromHeightMap(input.uv));

                float3x3 tangentToWorld = CreateTangentToWorld(input.normalWS, input.tangentWS.xyz, input.tangentWS.w);
                float3 normalWS = TransformTangentToWorld(normalTS, tangentToWorld);

                InputData lightingInput = (InputData)0; // Info about position and orientation of mesh at current fragment.
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = input.viewDir;
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                SurfaceData surfaceInput = (SurfaceData)0; // Holds info about the surface material's physical properties (e.g. color).
                surfaceInput.albedo = color.rgb;
                surfaceInput.alpha = color.a;
                surfaceInput.specular = 1;
                surfaceInput.metallic = 0;
                surfaceInput.smoothness = _Smoothness;
                surfaceInput.normalTS = normalTS;
                surfaceInput.occlusion = 1;
                
                return UniversalFragmentPBR(lightingInput, surfaceInput);
            }
            ENDHLSL
        }
    }
}