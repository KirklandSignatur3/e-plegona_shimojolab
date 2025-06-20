
#ifndef CURL_NOISE
#define CURL_NOISE

#include "SimplexNoiseGrad4D.cginc"

float3 curlNoise(float3 p, float noiseTime, float persistence) 
{
	float3 xNoisePotentialDerivatives = float3(0.0, 0.0, 0.0);
    float3 yNoisePotentialDerivatives = float3(0.0, 0.0, 0.0);
    float3 zNoisePotentialDerivatives = float3(0.0, 0.0, 0.0);

	for (int i = 0; i < 3; ++i) 
	{
        float twoPowI = pow(2.0, float(i));
        float scale = 0.5 * twoPowI * pow(persistence, float(i));
        xNoisePotentialDerivatives += snoise_grad(float4(p * twoPowI, noiseTime)) * scale;
        yNoisePotentialDerivatives += snoise_grad(float4((p + float3(123.4, 129845.6, -1239.1)) * twoPowI, noiseTime)) * scale;
        zNoisePotentialDerivatives += snoise_grad(float4((p + float3(-9519.0, 9051.0, -123.0)) * twoPowI, noiseTime)) * scale;
    }

    return float3(
        zNoisePotentialDerivatives[1] - yNoisePotentialDerivatives[2],
        xNoisePotentialDerivatives[2] - zNoisePotentialDerivatives[0],
        yNoisePotentialDerivatives[0] - xNoisePotentialDerivatives[1]
    );
}

#endif
