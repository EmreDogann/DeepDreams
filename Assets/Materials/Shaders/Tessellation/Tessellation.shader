Shader "Custom/Tessellation"
{
    Properties{
        [Header(Textures)] [Space] [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [NoScaleOffset] _FogCube("Fog Cube Texture", CUBE) = "" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
        [Header(Tessellation Factors)] [Space] _FactorEdge1("Edge factors", Vector) = (1, 1, 1, 0)
        //_FactorEdge2("Edge 2 factor", Float) = 1
        //_FactorEdge3("Edge 3 factor", Float) = 1
        _FactorInside("Inside factor", Float) = 1
        // This keyword enum allows us to choose between partitioning modes. It's best to try them out for yourself
        [KeywordEnum(INTEGER, FRAC_EVEN, FRAC_ODD, POW2)] _PARTITIONING("Partition algoritm", Float) = 0
        
        [Header(Other)] [Space] [Toggle(ENABLE_TRIPLANAR)] _Triplanar("Enable Triplanar Mapping", Float) = 0
        _TriplanarScale("Scale", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" "IgnoreProjector" = "True"}

        Pass
        {
            Name "Tessellation"
            HLSLPROGRAM

            #pragma target 5.0 // 5.0 required for tessellation.
            #pragma vertex Vertex
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment Fragment
            
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            // Material keywords
            #pragma shader_feature_local ENABLE_TRIPLANAR _PARTITIONING_INTEGER _PARTITIONING_FRAC_EVEN _PARTITIONING_FRAC_ODD _PARTITIONING_POW2

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 vertexOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct TessellationControlPoint
            {
                float3 positionWS : INTERNALTESSPOS; // POSITION semantic is forbidden in this structure, so use INTERNALTESSPOS instead.
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            //Depth Texture
            Texture2D _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;

            // Skybox cube texture
            TextureCube _FogCube;
            SamplerState sampler_FogCube;

            // CBUFFER section needed for SRP batching.
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _CameraDepthTexture_ST;
                float4 _BaseColor;
                float3 _FactorEdge1;
                float _FactorEdge2;
                float _FactorEdge3;
                float _FactorInside;
                float _TriplanarScale;
            CBUFFER_END

            TessellationControlPoint Vertex(Attributes input)
            {
                TessellationControlPoint output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.vertexOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            #define CONTROL_POINTS 3
            #define BEZIER_FACTORS_CONTROL_POINTS CONTROL_POINTS
            #define INPUT_TYPE "tri"

            // --------------------------------
            // ---------- Hull Stage ----------
            // --------------------------------
            #pragma region Hull

            // Hull function runs once per vertex.
            [domain(INPUT_TYPE)] // Signal we're inputting triangles.

            // Output patch type
            [outputcontrolpoints(CONTROL_POINTS)] // Triangles have 3 points.
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
            TessellationControlPoint Hull(InputPatch<TessellationControlPoint, CONTROL_POINTS> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
                // float3 bezierPoints[BEZIER_FACTORS_CONTROL_POINTS] : BEZIERPOS;
            };

            // The patch constant function runs once per patch and in parallel to the hull function.
            TessellationFactors PatchConstantFunction(InputPatch<TessellationControlPoint, CONTROL_POINTS> patch)
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]); // Setup instancing.

                TessellationFactors factors;
                factors.edge[0] = _FactorEdge1.x;
                factors.edge[1] = _FactorEdge1.y;
                factors.edge[2] = _FactorEdge1.z;
                factors.inside = _FactorInside;

                return factors;
            }
            #pragma endregion
            
            // ----------------------------------
            // ---------- Domain Stage ----------
            // ----------------------------------

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogCoords : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #define BARYCENTRIC_INTERPOLATE(fieldName) \
                patch[0].fieldName * barycentricCoordinates.x + \
                patch[1].fieldName * barycentricCoordinates.y + \
                patch[2].fieldName * barycentricCoordinates.z

            // The domain function runs once per vertex in the final, tessellated mesh.
            // Use it to reposition vertices and prepare for the fragment stage.
            [domain(INPUT_TYPE)] // Signal we're inputting triangles.
            Interpolators Domain(TessellationFactors factors, OutputPatch<TessellationControlPoint, CONTROL_POINTS> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                Interpolators output;

                // Setup instancing and stereo support (for VR)
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS_i = BARYCENTRIC_INTERPOLATE(positionWS);
                float3 normalWS_i = BARYCENTRIC_INTERPOLATE(normalWS);
                float2 uv_i = BARYCENTRIC_INTERPOLATE(uv);
                float3 viewDir_i =  -GetWorldSpaceNormalizeViewDir(positionWS_i);

                output.positionCS = TransformWorldToHClip(positionWS_i);
                output.normalWS = normalWS_i;
                output.positionWS = positionWS_i;
                output.uv = uv_i;
                output.viewDir = viewDir_i;

                output.fogCoords = ComputeFogFactor(output.positionCS.z);

                return output;
            }
            
            float4 Fragment(Interpolators input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                #if defined(ENABLE_TRIPLANAR)
                    // From: https://www.patreon.com/posts/quick-game-art-16714688
                    float3 uvs = input.positionWS.xyz * _TriplanarScale;
                    float3 blending = saturate(pow(input.normalWS.xyz * 1.4, 4));
                
                    float4 color = blending.z * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvs.xy);
                    color = lerp(color, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvs.yz), blending.x);
                    color = lerp(color, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvs.xz), blending.y);
                    color *= _BaseColor;
                #else
                    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                #endif

                float3 viewDir =  -GetWorldSpaceNormalizeViewDir(input.positionWS);
                float4 fogCube = SAMPLE_TEXTURECUBE(_FogCube, sampler_FogCube, viewDir);
                // color = lerp(color, fogCube, input.fogCoords);
                
                color = float4(MixFogColor(color.rgb, fogCube, input.fogCoords), color.a);
                // fixed4 col = tex2D(_MainTex, i.uv);
                return color;
            }
            ENDHLSL
        }
    }
}
