Shader "Hidden/Custom/EchoTrace"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
#include "Assets/CjLib/HLSL/Noise/SimplexNoise2D.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_PrevTex, sampler_PrevTex);

	float _Intensity;
	int _Difference;
	float _Gain;
	float _Threshold;
	int _Invert;

	float4 _MainTex_TexelSize;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 freshPixel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		float4 stalePixel = SAMPLE_TEXTURE2D(_PrevTex, sampler_PrevTex, i.texcoord);
		
		float brightLevel = (freshPixel.r + freshPixel.g + freshPixel.b) / 3.0;

		if (_Difference)
		{
			float4 diffPixel = freshPixel - stalePixel;
			brightLevel = (diffPixel.r + diffPixel.g + diffPixel.b) / 3.0;
		}

		if (_Invert > 0)  brightLevel = 1.0 - brightLevel;

		brightLevel = brightLevel * _Gain;

		if (brightLevel < _Threshold) brightLevel = 1.0;
		else brightLevel = 0.0;

		return lerp(freshPixel, stalePixel, brightLevel * _Intensity);
	}

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always

			Pass
		{

			HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment Frag

			ENDHLSL
		}

	}
}