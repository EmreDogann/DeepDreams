Shader "Custom/Water/Simulation"
{
	Properties
	{
		[HideInInspector] _A("A", Float) = 0
		[HideInInspector] _Attenuation("Attenuation", Float) = 0
		[HideInInspector] _Amplitude("Amplitude", Float) = 0
		[HideInInspector] _UVScale("UV Scale", Float) = 0
		[NoScaleOffset] _CollisionTex("CollisionTexture", 2D) = "black" {}
	}
	
	SubShader
	{
		Lighting Off
        Blend One Zero

		Pass
		{
			Name "Simulation Update"
			CGPROGRAM
			
            #include "UnityCustomRenderTexture.cginc"
			
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            // CBUFFER section needed for SRP batching.
            CBUFFER_START(UnityPerMaterial)
                // float _S2;
				float _A;
				float _Attenuation;
				float _Amplitude;
				float _UVScale;
				sampler2D _CollisionTex;
            CBUFFER_END

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
	            const float2 uv = IN.globalTexcoord;

            	// Get Texel Size.
            	float du = 1.0f / _CustomRenderTextureWidth;
            	float dv = 1.0f / _CustomRenderTextureHeight;
            	float3 duv = float3(du, dv, 0.0f) * _UVScale;

            	// _SelfTexture2D: Red Channel - Current simulation state.
            	// _SelfTexture2D: Green Channel - Previous simulation state.

            	// Run Simulation Step
     //        	float2 c = tex2D(_SelfTexture2D, uv);
     //        	float p = (2 * c.r - c.g + _S2 * (
					// tex2D(_SelfTexture2D, uv - duv.zy).r +
					// tex2D(_SelfTexture2D, uv + duv.zy).r +
					// tex2D(_SelfTexture2D, uv - duv.xz).r +
					// tex2D(_SelfTexture2D, uv + duv.xz).r - 4 * c.r
     //        	)) * _Attenuation;

            	// Run Simulation Step
            	float2 c = tex2D(_SelfTexture2D, uv);
            	float p = (2 * c.r - c.g + _A * (
					tex2D(_SelfTexture2D, uv - duv.zy).r +
					tex2D(_SelfTexture2D, uv + duv.zy).r +
					tex2D(_SelfTexture2D, uv - duv.xz).r +
					tex2D(_SelfTexture2D, uv + duv.xz).r - 4 * c.r
            	)) * _Attenuation;
            	
            	// Run Simulation Step - Alternative version
     //        	float2 c = tex2D(_SelfTexture2D, uv);
     //        	float p = _Attenuation * (_A * (
					// tex2D(_SelfTexture2D, uv + duv.xz).r +
					// tex2D(_SelfTexture2D, uv - duv.xz).r +
					// tex2D(_SelfTexture2D, uv + duv.zy).r +
					// tex2D(_SelfTexture2D, uv - duv.zy).r
     //        	) + (2.0f - 4.0f * _A) * c.r - c.g);

                const float prevCollision = tex2D(_SelfTexture2D, uv).b;
	            const float collision = tex2D(_CollisionTex, uv).r;
            	// if (collision > 0.0f && prevCollision == 0.0f)
            	// {
            	// 	p = _Amplitude * saturate(p - collision);
            	// } else if (collision == 0.0f && prevCollision > 0.0f)
            	// {
            	// 	p = _Amplitude * saturate(p + prevCollision);
            	// }

            	// If there was a collision where there was non previously, raise the water.
            	// if (collision > 0.0f && prevCollision == 0.0f)
            	// {
            	// 	p += collision * _Amplitude;
            	// }
	            //
            	// // If there was a previous collision where there isn't one now, lower the water.
            	// if (collision == 0.0f && prevCollision > 0.0f)
            	// {
            	// 	p -= prevCollision * _Amplitude;
            	// }

            	// If there was a collision where there was non previously, lower the water.
            	if (collision > 0.0f && prevCollision == 0.0f)
            	{
            		p -= collision * _Amplitude;
            	}
            	
                return float4(p, c.r, collision, 1.0f);
            }
			
            ENDCG
		}
	}
}