Shader "Hidden/Blur/KawaseDualFilterBlur" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_BlendTex ("Blend Texture", 2D) = "white" {}
		_BlueNoise ("Blue Noise Texture", 2D) = "white" {}
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
		#pragma target 3.0
		#pragma fragmentoption ARB_precision_hint_nicest
		// _ will create a shader variant where the keyword is disabled.
		// enable/disable keywords by doing _material.EnableKeyword()/_material.DisableKeyword().
		#pragma multi_compile_local_fragment _ ENABLE_DITHERING
		
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Assets/Materials/Shaders/Dither.hlsl"


		#define PI 3.14159265359
		#define E 2.71828182846

		#define DitheringScale          _Dithering_Params.xy
        #define DitheringOffset         _Dithering_Params.zw

		// glsl style mod
		#define mod(x, y) (x - y * floor(x / y))

		// Structs
		struct Attributes {
			float4 positionOS	: POSITION;
			float2 uv		: TEXCOORD0;
		};

		struct Varyings {
			float4 positionCS 	: SV_POSITION;
			float2 uv		: TEXCOORD0;
		};

		CBUFFER_START(UnityPerMaterial)
			// Textures, Samplers & Global Properties
			TEXTURE2D(_MainTex);
			float4 _MainTex_TexelSize;

			TEXTURE2D(_CameraOpaqueTexture);

			TEXTURE2D(_BlueNoise);
			float _BlurSize;
		
			float4 _Dithering_Params;
		CBUFFER_END
		
		// Vertex Shader
		Varyings vert(Attributes i) {
			Varyings o;

			const VertexPositionInputs positionInputs = GetVertexPositionInputs(i.positionOS.xyz);
			o.positionCS = positionInputs.positionCS;
			o.uv = i.uv;

			return o;
		}
		
		// Both Functions below from:
		// https://blog.en.uwa4d.com/2022/09/06/screen-post-processing-effects-chapter-5-dual-blur-and-its-implementation/
		float4 frag_downsample(Varyings i) : SV_Target
		{
			float4 offset = _MainTex_TexelSize.xyxy * float4(-_BlurSize,-_BlurSize,_BlurSize,_BlurSize);
			float4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv) * 4.0;

			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.xy); // Bottom left
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.xw); // Bottom right
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.zy); // Top left
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.zw); // Top right
			o *= 0.125f; // Divide by 8.

			return o;
		}
		
		float4 frag_upsample(Varyings i) : SV_Target
		{
			// i.uv = GetNormalizedScreenSpaceUV(i.positionCS);
			
			float4 offset = _MainTex_TexelSize.xyxy * float4(-_BlurSize,-_BlurSize,_BlurSize,_BlurSize);
			float4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + float2(offset.x, 0));

			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + float2(offset.z, 0));
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + float2(0, offset.y));
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + float2(0, offset.w));
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.xy / 2.0) * 2;
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.xw / 2.0) * 2;
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.zy / 2.0) * 2;
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, i.uv + offset.zw / 2.0) * 2;
			o *= 0.0833f; // Divide by 12.

			return o;
		}

		float4 frag_copy(Varyings i) : SV_TARGET
		{
			float4 color = SAMPLE_TEXTURE2D(_MainTex , sampler_LinearClamp, i.uv);
			#ifdef ENABLE_DITHERING
				ApplyDither(color, i.uv, TEXTURE2D_ARGS(_BlueNoise, sampler_PointRepeat), DitheringScale, i.positionCS.xy);
			#endif
			return _BlurSize < 1 ? lerp(SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_LinearClamp, i.uv), color, _BlurSize) : color;
		}

		ENDHLSL
		
		Pass {
			Name "Copy"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_copy
			
			ENDHLSL
		}

		Pass {
			Name "Downsample Pass"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_downsample
			
			ENDHLSL
		}
		
		Pass {
			Name "Upsample Pass"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_upsample
			
			ENDHLSL
		}
	}
}