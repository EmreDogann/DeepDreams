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
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            // Material keywords - Tessellation
            #pragma shader_feature_local _PARTITIONING_INTEGER _PARTITIONING_FRAC_EVEN _PARTITIONING_FRAC_ODD _PARTITIONING_POW2
            #pragma shader_feature_local _TESSELLATION_SMOOTHING_FLAT _TESSELLATION_SMOOTHING_PHONG _TESSELLATION_SMOOTHING_BEZIER_QUAD_NORMALS
            #pragma shader_feature_local _TESSELLATION_FACTOR_CONSTANT _TESSELLATION_FACTOR_WORLD _TESSELLATION_FACTOR_SCREEN _TESSELLATION_FACTOR_WORLD_WITH_DEPTH

            // Material keywords - Triplanar Mapping
            #pragma shader_feature_local ENABLE_TRIPLANAR
            #pragma shader_feature_local ENABLE_ALIGNED_TRIPLANAR
            #pragma shader_feature_local ENABLE_WIREFRAME

            // Material keywords - Lighting
            #define _SPECULAR_COLOR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

            #include "Assets/Materials/Shaders/Common.hlsl"
            #include "Tessellation.hlsl"

            struct Interpolators
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : INTERNALTESSPOS;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
                float2 uv_MainTex : TEXCOORD2;
                float3 positionOAS : TEXCOORD3;
                float3 normalOS : TEXCOORD4;
                float4 tangentWS : TEXCOORD5;
                float3 viewDir : TEXCOORD6;
                float fogCoords : TEXCOORD7;
                float2 barycentricCoordinates : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // #include "Wireframe.hlsl"

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
                output.lightMap = input.lightMap;
                
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
                
                // Apply height map.
                const float height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, output.uv, 0).r * _HeightMapAltitude;
                output.positionWS += output.normalWS * height;
                
                output.normalWS = normalWS;
                output.tangentWS = float4(tangentWS, patch[0].tangentWS.w);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.positionOAS *= scale; // OAS = Object Aligned Space. Apply only the scale to the object space vertex in order to compensate for rotation.
                output.fogCoords = ComputeFogFactor(output.positionCS.z);
                output.viewDir = GetWorldSpaceNormalizeViewDir(output.positionWS);

                float4 lightMap;
                BARYCENTRIC_INTERPOLATE_NOOUT(lightMap);
                OUTPUT_LIGHTMAP_UV(lightMap, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                
                return output;
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
                
                float3 flow = SAMPLE_TEXTURE2D(_WaterFlowMap, sampler_WaterFlowMap, input.uv * _Tiling).rgb;
			    flow.xy = flow.xy * 2 - 1; // Map is in 0-1 range. We have to map it back to -1-1 range using *2-1.
                flow *= _FlowStrength;
                float noise = SAMPLE_TEXTURE2D(_WaterFlowMap, sampler_WaterFlowMap, input.uv * _Tiling).a; // Greyscale noise in alpha channel.
			    float time = _Time.x * _Speed + noise;
                float2 jump = float2(_UJump, _VJump);
                float3 flowUVW_A = FlowUVW(input.uv, flow.xy, jump, _FlowOffset, _Tiling, time, false);
                float3 flowUVW_B = FlowUVW(input.uv, flow.xy, jump, _FlowOffset, _Tiling, time, true);
                
                float3 waterNormal1_A = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal1, sampler_WaterNormal1, flowUVW_A.xy * _NormalMapSize), _NormalMapStrength) * flowUVW_A.z;
                float3 waterNormal2_A = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal2, sampler_WaterNormal2, flowUVW_A.xy * _NormalMapSize - time * 2), _NormalMapStrength) * flowUVW_A.z;
                
                float3 waterNormal1_B = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal1, sampler_WaterNormal1, flowUVW_B.xy * _NormalMapSize), _NormalMapStrength) * flowUVW_B.z;
                float3 waterNormal2_B = UnpackNormalScale(SAMPLE_TEXTURE2D(_WaterNormal2, sampler_WaterNormal2, flowUVW_B.xy * _NormalMapSize - time * 2), _NormalMapStrength) * flowUVW_B.z;
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
                lightingInput.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, lightingInput.normalWS);

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