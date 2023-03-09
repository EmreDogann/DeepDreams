Shader "Custom/WaterLit"
{
    Properties
    {
        [Header(Textures)] [Space] [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [NoScaleOffset] _FogCube("Fog Cube Texture", CUBE) = "" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Smoothness("Smoothness", Float) = 0
        _NormalStrength("Normal Strength", Float) = 0
        
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
            Name "ForwardLit"
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

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            // Material keywords - Lighting
            #define _SPECULAR_COLOR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            Texture2D _MainTex; SamplerState sampler_MainTex;
            Texture2D _HeightMap; SamplerState sampler_HeightMap;
            TextureCube _FogCube; SamplerState sampler_FogCube; // Skybox cube texture

            // CBUFFER section needed for SRP batching.
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _Smoothness;

                float _NormalStrength;
                float4 _HeightMap_TexelSize;
                float _HeightMapAltitude;

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

            #include "Assets/Materials/Shaders/FastNoiseLite.hlsl"
            // #include "Wireframe.hlsl"

            // Create and configure noise state
            fnl_state InitFastNoise()
            {
                fnl_state noise = fnlCreateState();
                noise.noise_type = FNL_NOISE_OPENSIMPLEX2;
                return noise;
            }
            
            static fnl_state noise = InitFastNoise();

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
                float2 uvIncrement = _HeightMap_TexelSize * 4.0f;
                // float2 uvIncrement = float2(0.01, 0.01);
                // Sample the height from adjacent pixels.
                float left = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(-1, 0) * uvIncrement.x).r * _HeightMapAltitude;
                float right = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(1, 0) * uvIncrement.x).r * _HeightMapAltitude;
                float down = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(0, -1) * uvIncrement.y).r * _HeightMapAltitude;
                float up = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv + float2(0, 1) * uvIncrement.y).r * _HeightMapAltitude;

                // float left = textureBicubic(sampler_HeightMap, uv + float2(-1, 0) * uvIncrement.x).r * _HeightMapAltitude;
                // float right = textureBicubic(sampler_HeightMap, uv + float2(1, 0) * uvIncrement.x).r * _HeightMapAltitude;
                // float down = textureBicubic(sampler_HeightMap, uv + float2(0, -1) * uvIncrement.x).r * _HeightMapAltitude;
                // float up = textureBicubic(sampler_HeightMap, uv + float2(0, 1) * uvIncrement.x).r * _HeightMapAltitude;
                
                // float left = getTexel(uv + float2(-1, 0) * uvIncrement.x).r * _HeightMapAltitude;
                // float right = getTexel(uv + float2(1, 0) * uvIncrement.x).r * _HeightMapAltitude;
                // float down = getTexel(uv + float2(0, -1) * uvIncrement.y).r * _HeightMapAltitude;
                // float up = getTexel(uv + float2(0, 1) * uvIncrement.y).r * _HeightMapAltitude;
                
                // Generate a tangent space normal using the slope along the U and V axis.
                float3 normalTS = float3(
                    (left - right) / (uvIncrement.x * 2),
                    (down - up) / (uvIncrement.y * 2),
                    1
                );
                
                normalTS.xy *= _NormalStrength; // Adjust the XY channels to create stronger or weaker normals.
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
                height += wave(position, float2(1, 0), 0.05f, 2.0f, 1, _Time.y);
                // height += wave(position, float2(0, 1), 0.05f, 1.0f, 1, _Time.x);
                
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
                dx += dWavedx(position, float2(1, 0), 0.05f, 2.0f, 1, _Time.y);
                dy += dWavedy(position, float2(1, 0), 0.05f, 2.0f, 1, _Time.y);
                
                // dx += dWavedx(position, float2(0, 1), 0.05f, 1.0f, 1, _Time.x);
                // dy += dWavedy(position, float2(0, 1), 0.05f, 1.0f, 1, _Time.x);
                
                float3 n = float3(-dx, 1.0, -dy);
                return normalize(n);
            }

            struct Attributes
            {
                float3 vertexOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float2 uv_MainTex : TEXCOORD3;
                float4 tangentWS : TEXCOORD4;
                float fogCoords : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Interpolators Vertex(Attributes input)
            {
                Interpolators output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.vertexOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                // output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                // output.normalWS = normalInputs.normalWS;
                output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w); // tangent.w contains bitangent multiplier;
                output.uv = input.uv;
                output.uv_MainTex = TRANSFORM_TEX(input.uv, _MainTex);

                // // Create two fake neighbour vertices.
                // float3 v0 = output.positionWS;
                // float3 v1 = v0 + float3(0.001, 0, 0);
                // float3 v2 = v0 + float3(0, 0, 0.001);

                float3 bitangent = float3(1, 0, 0);
                float3 tangent   = float3(0, 0, 1);
                float offset = 0.01;

                float heightBitangent = waveHeight(output.positionWS.xz + (bitangent.xz * offset));
                float height = waveHeight(output.positionWS.xz);
                float heightTangent = waveHeight(output.positionWS.xz + (tangent.xz * offset));

                float3 bitangentVertex = output.positionWS + (bitangent * offset);
                bitangentVertex.y = heightBitangent;

                float3 originalVertex = output.positionWS;
                originalVertex.y = height;

                float3 tangentVertex = output.positionWS + (tangent * offset);
                tangentVertex.y = heightTangent;

                float3 newBitangent = (bitangentVertex - originalVertex);
                float3 newTangent = (tangentVertex - originalVertex);

                output.normalWS = normalize(cross(newTangent, newBitangent));

                // noise.octaves = 1;
                // noise.frequency = 1.0f;
                // output.positionWS += normalWS * fnlGetNoise2D(noise, output.positionWS.x, output.positionWS.z  + _Time.y / 5) / 8;
                // noise.octaves = 5;
                // noise.frequency = 0.5f;
                // output.positionWS += normalWS * fnlGetNoise2D(noise, output.positionWS.x + _Time.y / 5, output.positionWS.z) / 8;
                // output.normalWS = waveNormal(output.positionWS.xz);
                
                // Apply height map.
                // const float height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, output.uv, 0).r * _HeightMapAltitude;
                // output.positionWS += output.normalWS * height;
                
                // output.normalWS = waveNormal(output.positionWS.xz);
                // const float height1 = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, v1.xz, 0).r * _HeightMapAltitude;
                // v1 += output.normalWS * height1;
                //
                // const float height2 = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, v2.xz, 0).r * _HeightMapAltitude;
                // v2 += output.normalWS * height2;
                //
                // float3 vna = cross(v2-v0, v1-v0);
                // output.normalWS = vna;

                output.positionWS = positionInputs.positionWS + height;
                output.positionCS = TransformWorldToHClip(output.positionWS);
                // normalWS = waveNormal(output.positionWS.xz);
                // output.tangentWS = float4(tangentWS, patch[0].tangentWS.w);
                // Apply only the scale to the object space vertex in order to compensate for rotation.
                output.fogCoords = ComputeFogFactor(output.positionCS.z);

                return output;
            }
            
            float4 Fragment(Interpolators input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv_MainTex);
                color *= _BaseColor;

                // Fog Reference: https://github.com/keijiro/KinoFog/blob/master/Assets/Kino/Fog/Shader/Fog.shader
                // https://www.reddit.com/r/Unity3D/comments/7wm02n/perfect_fog_for_your_game_trouble_matching_fog/
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                // float4 fogCube = SAMPLE_TEXTURECUBE(_FogCube, sampler_FogCube, -viewDir);
                
                // color = float4(MixFogColor(color.rgb, fogCube.rgb, input.fogCoords), color.a);

                float3x3 tangentToWorld = CreateTangentToWorld(input.normalWS, input.tangentWS.xyz, input.tangentWS.w);
                float3 normalTS = GenerateNormalFromHeightMap(input.uv);
                // float3 normalWS = normalize(TransformTangentToWorld(normalTS, tangentToWorld));
                float3 normalWS = waveNormal(input.positionWS.xz);

                InputData lightingInput = (InputData)0; // Info about position and orientation of mesh at current fragment.
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalize(input.normalWS);
                // lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = viewDir;
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                SurfaceData surfaceInput = (SurfaceData)0; // Holds info about the surface material's physical properties (e.g. color).
                surfaceInput.albedo = color.rgb;
                surfaceInput.alpha = color.a;
                surfaceInput.specular = 1;
                surfaceInput.metallic = 0;
                surfaceInput.smoothness = _Smoothness;
                // surfaceInput.normalTS = normalTS;
                // surfaceInput.occlusion = 1;
                
                // return color;
                return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
            }
            ENDHLSL
        }
    }
}