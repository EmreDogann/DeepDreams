Shader "Custom/Water/Simulation"
{
	Properties
	{
		_S2("PhaseVelocity^2", Range(0.0, 0.5)) = 0.2
		[PowerSlider(0.01)] _Attenuation("Attenuation", Range(0.0, 1.0)) = 0.999
		_DeltaUV("Delta UV", Float) = 3
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
                float _S2;
				float _Attenuation;
				float _DeltaUV;
				sampler2D _CollisionTex;
            CBUFFER_END

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
	            const float2 uv = IN.globalTexcoord;

            	// Get Texel Size.
            	float du = 1.0 / _CustomRenderTextureWidth;
            	float dv = 1.0 / _CustomRenderTextureHeight;
            	float3 duv = float3(du, dv, 0) * _DeltaUV;

            	// Run Simulation Step
            	float2 c = tex2D(_SelfTexture2D, uv);
            	float p = (2 * c.r - c.g + _S2 * (
					tex2D(_SelfTexture2D, uv - duv.zy).r +
					tex2D(_SelfTexture2D, uv + duv.zy).r +
					tex2D(_SelfTexture2D, uv - duv.xz).r +
					tex2D(_SelfTexture2D, uv + duv.xz).r - 4 * c.r
            	)) * _Attenuation;

                const float prevCollision = tex2D(_SelfTexture2D, uv).b;
            	float collision = tex2D(_CollisionTex, uv).r;
            	if (collision > 0.0 && prevCollision == 0.0)
            	{
            		p = saturate(p - collision);
            	}
            	if (prevCollision > 0.0 && collision == 0.0)
            	{
            		p = saturate(p + prevCollision);
            	}
            	
                return float4(p, c.r, collision, 0);
            }
			
            ENDCG
		}
	}
}