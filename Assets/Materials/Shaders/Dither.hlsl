#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// ----------------------------------------------------------------------------------
// Samplers

SAMPLER(sampler_LinearClamp);
SAMPLER(sampler_LinearRepeat);
SAMPLER(sampler_PointClamp);
SAMPLER(sampler_PointRepeat);

// Turns a linear RGB color value into sRGB.
float3 LinearToSRGB(float3 LinearRGB)
{
	return (LinearRGB <= 0.0031308f) ? (12.92f * LinearRGB) : mad(1.055f, pow(LinearRGB, 1.0f / 2.4f), -0.055f);
}

// Turns an sRGB color value into a linear RGB color value.
float3 SRGBToLinear(float3 SRGB)
{
	return (SRGB <= 0.04045f) ? (SRGB / 12.92f) : pow(mad(SRGB, 1.0f / 1.055f, 0.055f / 1.055f), 2.4f);
}

// https://www.shadertoy.com/view/4djSRW
float hash11(float p)
{
	p = frac(p * .1031);
	p *= p + 33.33;
	p *= p + p;
	return frac(p);
}

float hash12(float2 p)
{
	float3 p3 = frac(float3(p.xyx) * .1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return frac((p3.x + p3.y) * p3.z);
}

// void ComputeDitheredScatteringColor(out float4 OutScatteringColor, float3 ScatteringRadiance,
// 	Texture2DArray<float4> BlueNoiseTexture, uint2 SourceTexelIndex, uint4 Randomness)
// {
// 	// Get some blue noise
// 	float3 BlueNoise = BlueNoiseTexture.Load(uint4((SourceTexelIndex+Randomness.xy)&0x3F,Randomness.w&0x3F,0)).rgb;
// 			
// 	// Go from a uniform distribution on [0,1] to a 
// 	// symmetric triangular distribution on [-1,1] 
// 	// with maximal density at 0
// 	BlueNoise=mad(BlueNoise,2.0f,-1.0f);
// 	BlueNoise=sign(BlueNoise)*(1.0f-sqrt(1.0f-abs(BlueNoise)));
// 			
// 	// The dithering has to be done in the same 
// 	// space as quantization which is sRGB
// 	OutScatteringColor.rgb=SRGBToLinear(LinearToSRGB(ScatteringRadiance)+BlueNoise/255.0f);
// 	OutScatteringColor.a=1.0f;
// }

void ApplyDither(inout float4 color, float2 uv, TEXTURE2D_PARAM(BlueNoiseTexture, BlueNoiseSampler), float2 scale, float2 offset)
{
	const float2 scaledUV = uv * scale;
	float3 BlueNoise = SAMPLE_TEXTURE2D(BlueNoiseTexture, BlueNoiseSampler, scaledUV + hash11(_Time.x)).rgb;

	// Go from a uniform distribution on [0,1] to a symmetric triangular distribution on [-1,1] with maximal density at 0.
	BlueNoise = mad(BlueNoise, 2.0f, -1.0f);
	BlueNoise = sign(BlueNoise) * (1.0f - sqrt(1.0f - abs(BlueNoise)));

	#if UNITY_COLORSPACE_GAMMA
	color.rgb += BlueNoise / 255.0;
	#else
	color.rgb = SRGBToLinear(LinearToSRGB(color) + BlueNoise / 255.0);
	#endif

	// color = max(color, 0);
}
