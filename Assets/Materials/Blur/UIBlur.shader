Shader "Custom/Blur/UIBlur" {
	Properties {
		// --- Mask support ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}
	
	SubShader {
		Tags {
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		
		// --- Mask support ---
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
		
		Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		
		// No culling or depth
		Cull off
		ZWrite off
		ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]
		Lighting Off

		HLSLINCLUDE
		#pragma target 3.0
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
		#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
		
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		// Structs
		struct Attributes {
			float4 positionOS	: POSITION;
			float2 uv		: TEXCOORD0;

			// https://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
			// https://answers.unity.com/questions/1441060/access-ui-image-color-in-shader.html
			// Vertex color. UI Image fills this with its "Color" field.
			// Also contains alpha needed for correct Ui alpha blending.
			float4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Varyings {
			float4 positionCS 	: SV_POSITION;
			float2 uv		: TEXCOORD0;
			float3 positionWS 	: TEXCOORD1;
			float4 color : COLOR;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		CBUFFER_START(UnityPerMaterial)
			// Textures, Samplers & Global Properties
			TEXTURE2D(_BlurTexture);
			float4 _BlurTexture_TexelSize;

			TEXTURE2D(_CameraOpaqueTexture);

			// ----------------------------------------------------------------------------------
			// Samplers
			SAMPLER(sampler_LinearClamp);
			SAMPLER(sampler_LinearRepeat);
			SAMPLER(sampler_PointClamp);
			SAMPLER(sampler_PointRepeat);

			float4 _WidthHeightRadius;
            float4 _ClipRect;
		CBUFFER_END
		
		// Vertex Shader
		Varyings vert(Attributes i) {
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
			
			Varyings o;
			const VertexPositionInputs positionInputs = GetVertexPositionInputs(i.positionOS.xyz);
			o.positionCS = positionInputs.positionCS;
			o.positionWS = i.positionOS;
			o.uv = i.uv;
			o.color = i.color;

			return o;
		}

		float4 frag(Varyings i) : SV_TARGET
		{
			float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS.xy);
			#if UNITY_UV_STARTS_AT_TOP
				if (_ProjectionParams.x == 1)
				{
					screenUV.y = 1 - screenUV.y;
				}
			#endif

			// Canvas Group's alpha: http://answers.unity.com/answers/1159971/view.html
			// CanvasGroup dictates transparency using the alpha in the vertex color.
			// So to apply the alpha field, we need to multiply by its vertex color alpha.
			float4 color = SAMPLE_TEXTURE2D(_BlurTexture , sampler_LinearClamp, screenUV) * i.color;

			#ifdef UNITY_UI_CLIP_RECT
			float2 inside = step(_ClipRect.xy, i.positionWS.xy) * step(i.positionWS.xy, _ClipRect.zw);
			color.a *= inside.x * inside.y;
			// half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
            // color *= m.x * m.y;
            #endif
   
            #ifdef UNITY_UI_ALPHACLIP
            clip(color.a - 0.001);
            #endif
   
			// if (color.a <= 0) {
   //              return color;
   //          }
			
			return color;
		}

		ENDHLSL
		
		Pass {
			Name "Copy"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			ENDHLSL
		}
	}
}