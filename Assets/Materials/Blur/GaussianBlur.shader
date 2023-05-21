Shader "Hidden/Blur/GaussianBlur" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_Iterations("Iterations", Integer) = 0
		_BlurSize("Blur Size", Range(0,0.5)) = 0
		[PowerSlider(3)] _StandardDeviation("Standard Deviation (Gauss only)", Range(0.00, 0.3)) = 0.02
	}
	SubShader {
		Tags {
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}
		
		// No culling or depth
		Cull off ZWrite off ZTest Always

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Assets/Materials/Shaders/Dither.hlsl"

		#pragma vertex vert
		#pragma fragment frag

		// _ will create a shader variant where the keyword is disabled.
		// enable/disable keywords by doing _material.EnableKeyword()/_material.DisableKeyword().
		#pragma multi_compile_local_fragment _ ENABLE_DITHERING

		#define PI 3.14159265359
		#define E 2.71828182846

		#define DitheringScale          _Dithering_Params.xy
        #define DitheringOffset         _Dithering_Params.zw

		// Structs
		struct Attributes {
			float4 positionOS	: POSITION;
			float2 uv		: TEXCOORD0;
		};

		struct Varyings {
			float4 positionCS 	: SV_POSITION;
			float2 uv		: TEXCOORD0;
		};

		// Textures, Samplers & Global Properties
		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		// Vertex Shader
		Varyings vert(Attributes i) {
			Varyings o;

			const VertexPositionInputs positionInputs = GetVertexPositionInputs(i.positionOS.xyz);
			o.positionCS = positionInputs.positionCS;
			o.uv = i.uv;
			return o;
		}

		CBUFFER_START(UnityPerMaterial)
			int _Iterations;
			float _BlurSize;
			float _StandardDeviation;

			TEXTURE2D(_BlueNoise);
			float4 _Dithering_Params;
		CBUFFER_END

		ENDHLSL

		Pass {
			Name "Vertical Pass"

			HLSLPROGRAM
			// Fragment Shader
			float4 frag(Varyings i) : SV_Target {

				// failsafe so we can turn off the blur by setting the deviation to 0.
				if (_StandardDeviation == 0)
				{
					return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				}

				float4 color = 0;
				float sum = 0;

				for (float index = 0; index < _Iterations; index++)
				{
					float offset = (index / (_Iterations - 1.0) - 0.5) * _BlurSize;

					const float2 uv = i.uv + float2(0, offset);

					const float variance = _StandardDeviation * _StandardDeviation;
					const float gaussian = (1 / sqrt(2 * PI * variance)) * pow(E, -((offset * offset) / (2 * variance)));

					sum += gaussian;

					color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * gaussian;
				}

				color = color / sum;

				#ifdef ENABLE_DITHERING
				ApplyDither(color, i.uv, TEXTURE2D_ARGS(_BlueNoise, sampler_PointRepeat), DitheringScale, i.positionCS.xy);
				#endif

				return color;
			}
			ENDHLSL
		}
		
		Pass {
			Name "Horizontal Pass"

			HLSLPROGRAM
			// Fragment Shader
			float4 frag(Varyings i) : SV_Target {

				// failsafe so we can turn off the blur by setting the deviation to 0.
				// Gaussian blur breaks at 0.
				if (_StandardDeviation == 0)
				{
					return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				}

				//calculate aspect ratio
				const float invAspect = _ScreenParams.y / _ScreenParams.x;

				float4 color = 0;
				float sum = 0;

				for (float index = 0; index < _Iterations; index++)
				{
					float offset = (index / (_Iterations - 1.0) - 0.5) * _BlurSize;

					const float2 uv = i.uv + float2(offset, 0) * invAspect;

					const float variance = _StandardDeviation * _StandardDeviation;
					const float gaussian = (1 / sqrt(2 * PI * variance)) * pow(E, -((offset * offset) / (2 * variance)));

					sum += gaussian;

					color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * gaussian;
				}

				color = color / sum;
				
				#ifdef ENABLE_DITHERING
				ApplyDither(color, i.uv, TEXTURE2D_ARGS(_BlueNoise, sampler_PointRepeat), DitheringScale, i.positionCS.xy);
				#endif

				return color;
			}
			ENDHLSL
		}
	}
}