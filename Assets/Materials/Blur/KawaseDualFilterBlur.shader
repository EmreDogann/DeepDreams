Shader "Hidden/Blur/KawaseDualFilterBlur" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
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
		#pragma fragmentoption ARB_precision_hint_fastest
		// _ will create a shader variant where the keyword is disabled.
		// enable/disable keywords by doing _material.EnableKeyword()/_material.DisableKeyword().
		#pragma multi_compile_local_fragment _ ENABLE_DITHERING
		#pragma multi_compile_local_fragment _ _HQ_FILTERING
		
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Assets/Materials/Shaders/Dither.hlsl"

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

		CBUFFER_START(UnityPerMaterial)
			// Textures, Samplers & Global Properties
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;

			TEXTURE2D(_CameraOpaqueTexture);
			SAMPLER(sampler_CameraOpaqueTexture);

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

		float3 PowVec3(float3 v, float p)
		{
		    return float3(pow(v.x, p), pow(v.y, p), pow(v.z, p));
		}

		// const float invGamma = ;
		float3 ToSRGB(float3 v) { return PowVec3(v, 1.0f / 2.2f); }

		float RGBToLuminance(float3 col)
		{
		    return dot(col, float3(0.2126f, 0.7152f, 0.0722f));
		}

		float KarisAverage(float3 col)
		{
		    // Formula is 1 / (1 + luma)
		    float luma = RGBToLuminance(ToSRGB(col)) * 0.25f;
		    return 1.0f / (1.0f + luma);
		}

		float4 frag_downsample(Varyings i) : SV_Target
		{
		#ifdef _HQ_FILTERING
			// Better, temporally stable box filtering
			// [Jimenez14] http://goo.gl/eomGso
			// . . . . . . .
			// . A . B . C .
			// . . D . E . .
			// . F . G . H .
			// . . I . J . .
			// . K . L . M .
			// . . . . . . .
			float2 texelSize = _MainTex_TexelSize.xy;

			float4 A = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-1.0, -1.0));
		    float4 B = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.0, -1.0));
		    float4 C = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 1.0, -1.0));
		    float4 D = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-0.5, -0.5));
		    float4 E = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.5, -0.5));
		    float4 F = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-1.0,  0.0));
		    float4 G = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv                                 );
		    float4 H = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 1.0,  0.0));
		    float4 I = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-0.5,  0.5));
		    float4 J = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.5,  0.5));
		    float4 K = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-1.0,  1.0));
		    float4 L = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.0,  1.0));
		    float4 M = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 1.0,  1.0));

			float2 div = (1.0 / 4.0) * half2(0.5, 0.125);

			float4 o = (D + E + I + J) * div.x;
			o += (A + B + G + F) * div.y;
			o += (B + C + H + G) * div.y;
			o += (F + G + L + K) * div.y;
			o += (G + H + M + L) * div.y;
		#else
			// https://blog.en.uwa4d.com/2022/09/06/screen-post-processing-effects-chapter-5-dual-blur-and-its-implementation/
			// Average 16-texel box filtering (4 bilinear fetches)
			float4 offset = _MainTex_TexelSize.xyxy * float4(-1,-1,1,1);
			float4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 4.0;
			
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xy); // Bottom left
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xw); // Bottom right
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zy); // Top left
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zw); // Top right
			o *= 0.125f; // Divide by 8.
		#endif

			return o;
		}

		float4 frag_downsample_with_karis(Varyings i) : SV_Target
		{
		#ifdef _HQ_FILTERING
			float2 texelSize = _MainTex_TexelSize.xy;

			float4 A = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-1.0, -1.0));
		    float4 B = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.0, -1.0));
		    float4 C = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 1.0, -1.0));
		    float4 D = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-0.5, -0.5));
		    float4 E = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.5, -0.5));
		    float4 F = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-1.0,  0.0));
		    float4 G = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv                                 );
		    float4 H = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 1.0,  0.0));
		    float4 I = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-0.5,  0.5));
		    float4 J = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.5,  0.5));
		    float4 K = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2(-1.0,  1.0));
		    float4 L = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 0.0,  1.0));
		    float4 M = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + texelSize * float2( 1.0,  1.0));
			
			// Karis's luma weighted average (using brightness instead of luma)
			float4 groups[5];
			groups[0] = (D + E + I + J) * 0.25f;
		    groups[1] = (A + B + G + F) * 0.25f;
		    groups[2] = (B + C + H + G) * 0.25f;
		    groups[3] = (F + G + L + K) * 0.25f;
		    groups[4] = (G + H + M + L) * 0.25f;
			
			float4 kw0 = KarisAverage(groups[0].rgb);
			kw0.a = groups[0].a;
			float4 kw1 = KarisAverage(groups[1].rgb);
			kw1.a = groups[1].a;
			float4 kw2 = KarisAverage(groups[2].rgb);
			kw2.a = groups[2].a;
			float4 kw3 = KarisAverage(groups[3].rgb);
			kw3.a = groups[3].a;
			float4 kw4 = KarisAverage(groups[4].rgb);
			kw4.a = groups[4].a;
			
			float4 o = (kw0 * groups[0] + kw1 * groups[1] + kw2 * groups[2] + kw3 * groups[3] + kw4 * groups[4]) / (kw0 + kw1 + kw2 + kw3 + kw4);
			// Prevents NaN/black pixel propagation.
			o = max(o, 0.0001f);
		#else
			float4 offset = _MainTex_TexelSize.xyxy * float4(-1,-1,1,1);
			float4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 4.0;
			
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xy); // Bottom left
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xw); // Bottom right
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zy); // Top left
			o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zw); // Top right
			o *= 0.125f; // Divide by 8.
		#endif

			return o;
		}

		float4 frag_upsample(Varyings i) : SV_Target
		{
			// i.uv = GetNormalizedScreenSpaceUV(i.positionCS);

			#ifdef _HQ_FILTERING
				// 9-tap bilinear upsampler (tent filter)
				// [ 1 2 1 ]
				// [ 2 4 2 ] x ( 1 / 16)
				// [ 1 2 1 ]
			    float4 d = _MainTex_TexelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * _BlurSize;

			    half4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - d.xy);
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - d.wy) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - d.zy);
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.zw) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv)        * 4.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.xw) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.zy);
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.wy) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.xy);
				o *= (1.0 / 16.0);
			#else
				float4 offset = _MainTex_TexelSize.xyxy * float4(-1,-1,1,1) * _BlurSize;
				float4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(offset.x, 0));
				
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(offset.z, 0));
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, offset.y));
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, offset.w));
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xy / 2.0) * 2;
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xw / 2.0) * 2;
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zy / 2.0) * 2;
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zw / 2.0) * 2;
				o *= 0.0833f; // Divide by 12.
			#endif

			return o;
		}

		float4 frag_final_upsample(Varyings i) : SV_Target
		{
			#ifdef _HQ_FILTERING
				// 9-tap bilinear upsampler (tent filter)
			    float4 d = _MainTex_TexelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * _BlurSize;

			    half4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - d.xy);
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - d.wy) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - d.zy);
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.zw) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv)        * 4.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.xw) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.zy);
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.wy) * 2.0;
			    o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d.xy);
				o *= (1.0 / 16.0);
			#else
				float4 offset = _MainTex_TexelSize.xyxy * float4(-1,-1,1,1) * _BlurSize;
				float4 o = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(offset.x, 0));
				
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(offset.z, 0));
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, offset.y));
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, offset.w));
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xy / 2.0) * 2;
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.xw / 2.0) * 2;
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zy / 2.0) * 2;
				o += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset.zw / 2.0) * 2;
				o *= 0.0833f; // Divide by 12.
			#endif

			#ifdef ENABLE_DITHERING
			ApplyDither(o, i.uv, TEXTURE2D_ARGS(_BlueNoise, sampler_PointRepeat), DitheringScale, i.positionCS.xy);
			#endif
			return _BlurSize < 1 ? lerp(SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv), o, _BlurSize) : o;
		}

		float4 frag_copy(Varyings i) : SV_TARGET
		{
			return SAMPLE_TEXTURE2D(_MainTex , sampler_MainTex, i.uv);
		}

		ENDHLSL
		
		Pass { // Pass 0
			Name "Copy"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_copy
			
			ENDHLSL
		}
		
		Pass { // Pass 1
			Name "Downsample Pass w/ Karis Average"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_downsample_with_karis
			
			ENDHLSL
		}

		Pass { // Pass 2
			Name "Downsample Pass"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_downsample
			
			ENDHLSL
		}
		
		Pass { // Pass 3
			Name "Upsample Pass"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_upsample
			
			ENDHLSL
		}
		
		Pass { // Pass 4
			Name "Final Upsample Pass"

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag_final_upsample
			
			ENDHLSL
		}
	}
}