Shader "Custom/MainLightVolumetric"
{
    Properties
    {
        [Header(Textures)] [Space] [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }
    
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "PreviewType" = "Plane"
            "ShaderModel" = "4.5"
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

            // Material keywords - Lighting
            #define _SPECULAR_COLOR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            Texture2D _MainTex; SamplerState sampler_MainTex;
            TextureCube _FogCube; SamplerState sampler_FogCube; // Skybox cube texture

            // CBUFFER section needed for SRP batching.
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float3 vertexOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 lightMap : TEXCOORD1;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // #include "Wireframe.hlsl"

            Interpolators Vertex(Attributes input)
            {
                Interpolators output = (Interpolators)0;
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
                // output.fogCoords = ComputeFogFactor(output.positionCS.z);
                output.viewDir = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                OUTPUT_LIGHTMAP_UV(input.lightMap, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                
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
                // float4 fogCube = SAMPLE_TEXTURECUBE(_FogCube, sampler_FogCube, -viewDir);
                
                // color = float4(MixFogColor(color.rgb, fogCube.rgb, input.fogCoords), color.a);

                InputData lightingInput = (InputData)0; // Info about position and orientation of mesh at current fragment.
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = input.normalWS;
                lightingInput.viewDirectionWS = input.viewDir;
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                lightingInput.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, lightingInput.normalWS);

                SurfaceData surfaceInput = (SurfaceData)0; // Holds info about the surface material's physical properties (e.g. color).
                surfaceInput.albedo = color.rgb;
                surfaceInput.alpha = color.a;
                surfaceInput.specular = 1;
                surfaceInput.metallic = 0;
                surfaceInput.smoothness = 0.5;
                // surfaceInput.normalTS = normalTS;
                surfaceInput.occlusion = 1;
                
                return UniversalFragmentPBR(lightingInput, surfaceInput);
            }
            ENDHLSL
        }
    }
}