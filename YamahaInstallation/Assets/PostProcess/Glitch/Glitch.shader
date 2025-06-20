Shader "Hidden/Custom/Glitch"
{
	HLSLINCLUDE

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
	#include "Assets/CjLib/HLSL/Noise/SimplexNoise2D.hlsl"
	
	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	
	float _Intensity;
	float _Scale;
	float _Speed;
	float _Seed;

	float nrand( float2 n )
	{
		return frac(sin(dot(n.xy, float2(12.9898, 78.233)))* 43758.5453);
	}
	
	float n2rand( float2 n )
	{
		float t = frac( _Time );
		float nrnd0 = nrand( n + 0.07*t );
		float nrnd1 = nrand( n + 0.11*t );
		return (nrnd0+nrnd1) / 2.0;
	} 
	float4 _MainTex_TexelSize;
//https://www.shadertoy.com/view/XtyXzW
	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float noiseScale = _Intensity * 100;
		float t = (float)_Time * _Speed * 0.001;

		float2 uv = i.texcoord;
		uv.y *= _MainTex_TexelSize.w / _MainTex_TexelSize.z;
		uv.y *= 8.0;
		float2 uvN = floor(uv * int(noiseScale)) / int(noiseScale);

		float4 result = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		float n = snoise(uvN + float2(_Seed, 0.0), nrand(float2(t, 0)), 2.0, 1.0);	


		if (n < 0.4)
		{
			float p1 = floor(nrand(uvN + t) * 4.0) / 4.0;
			result = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mod(i.texcoord + float2((p1 - 0.5) *0.1, 0.0), 1.0));
			
			float2 uv2N = floor(uv * int(noiseScale) * 2.0) / (int(noiseScale) * 2.0) * 2.0;
			float n2 = snoise(uv2N + float2(_Seed, 30.0), nrand(float2(t, 0)), 1.0, 1.0);
			
			if (n2 < 0.3)
			{
				float p2 = floor(nrand(uv2N + t * 2) * 4.0) / 4.0;
				result = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mod(i.texcoord + float2((p2-0.5)*0.05, 0.0), 1.0)) - float4(1.0, 1.0, 1.0, 0.0);
			}
		}



/*
		float noise = _Intensity * n * 3.0;
		float noise2 = noise - 0.5;
		if (noise2 < 0.0) noise2 = 0.0;

		t = (float)_Time * 0.0 * _Speed;
		float n1 = snoise(float2(0.0, i.texcoord.y * noiseScale * 10), float2(t, t), 2.0, 1.0);
		t = (float)_Time * 10.0 * _Speed;
		float n2 = snoise(float2(0.0, i.texcoord.y * noiseScale * 100), float2(t, t), 3.0, 1.0);
		t = (float)_Time * 100.0 * _Speed;
		float n3 = snoise(float2(0.0, i.texcoord.y * noiseScale * 2000), float2(t, t), 1.0, 1.0);
		n2 -= 0.4;
		if (n2 < 0.0) n2 = 0.0;

		//float2 resultUV =  i.texcoord + float2((n1 + (n2 * n3) - 0.5 - n3 * 0.5), 0.0) * noise;
		float2 resultUV = fmod(i.texcoord + float2(n1 * noise + n2 * n3 * noise, 1.0), 1.0);// +float2(((n1 + n2)* noise + (n3 - 0.5 * n2) * noise2), 0.0), 1.0);
		float black = _Intensity * 0.1;
		
		float4 result = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, resultUV);// * (1.0 - noise * 0.5)// + 
		//n2rand(resultUV) * noise * 0.5 - float4(black, black, black, 1.0);
		result.a = 1.0;
	*/
		


		

		

		
		return result;
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