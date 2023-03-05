Shader "Custom/CustomUnlit"
{
    Properties
    {
        [Header(Textures)] [Space] [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [NoScaleOffset] _FogCube("Fog Cube Texture", CUBE) = "" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
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
            "LightMode" = "SRPDefaultUnlit"
            "Queue" = "Geometry"
            "PreviewType" = "Plane"
            "ShaderModel" = "4.5"
        }
        LOD 100
        
        Pass
        {
            Name "Tessellation"
            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask RGB
            
            HLSLPROGRAM
            #pragma target 4.5
            
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            // Material keywords
            #pragma shader_feature_local ENABLE_TRIPLANAR
            #pragma shader_feature_local ENABLE_ALIGNED_TRIPLANAR
            #pragma shader_feature_local ENABLE_WIREFRAME

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            Texture2D _MainTex; SamplerState sampler_MainTex;
            TextureCube _FogCube; SamplerState sampler_FogCube; // Skybox cube texture

            // CBUFFER section needed for SRP batching.
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
            
                float _TriplanarScale;
                float _TriplanarBlendOffset;
                float _TriplanarBlendSharpness;

                float4 _WireframeColor;
                float _WireframeSmoothing;
                float _WireframeThickness;

                // Extract scale from object to world matrix.
                static float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
            CBUFFER_END
            

            struct Attributes
            {
                float3 vertexOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : INTERNALTESSPOS;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 positionOAS : TEXCOORD1;
                float3 normalOS : TEXCOORD2;
                float fogCoords : TEXCOORD3;
                float2 barycentricCoordinates : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            #include "Wireframe.hlsl"

            Interpolators Vertex(Attributes input)
            {
                // Setup instancing and stereo support (for VR)
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                const VertexPositionInputs positionInputs = GetVertexPositionInputs(input.vertexOS);
                const VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                Interpolators output = (Interpolators)0;
                // OAS - Object Aligned Space
                // Apply only the scale to the object space vertex in order to compensate for rotation.
                output.positionOAS = input.vertexOS.xyz * scale;
                output.normalOS = input.normalOS.xyz;

                output.positionCS = TransformWorldToHClip(positionInputs.positionWS);
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                output.fogCoords = ComputeFogFactor(output.positionCS.z);
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
                    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                #endif
                color *= _BaseColor;

                // Fog Reference: https://github.com/keijiro/KinoFog/blob/master/Assets/Kino/Fog/Shader/Fog.shader
                // https://www.reddit.com/r/Unity3D/comments/7wm02n/perfect_fog_for_your_game_trouble_matching_fog/
                float3 viewDir = -GetWorldSpaceNormalizeViewDir(input.positionWS);
                float4 fogCube = SAMPLE_TEXTURECUBE(_FogCube, sampler_FogCube, viewDir);
                
                color = float4(MixFogColor(color.rgb, fogCube.rgb, input.fogCoords), color.a);

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
                
                return color;
            }
            ENDHLSL
        }
    }
}
