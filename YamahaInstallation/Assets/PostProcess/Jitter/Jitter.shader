Shader "Hidden/Custom/Jitter"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        #include "Assets/CjLib/HLSL/Noise/SimplexNoise2D.hlsl"
        
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        
        float _Intensity;
		float _Scale;
		float _Speed;
		float _Threshold;
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
        
		float4 Frag(VaryingsDefault i) : SV_Target
		{
			float noiseScale = _Scale * 2.0;



		float contrast = 10.0;
		float con = 1 / (1 + exp(-contrast * (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord).r - 0.5)));
		con = 1.0 - con;
		if (con < _Threshold)
		{
			con = 1.0;
		}
		else {
			con = 0.0;
		}

			float t = (float)_Time;
			float n = snoise(i.texcoord * 0.1, float2(t, t), 3.0, 1.0) * 0.5 + 0.5;
			float noise = _Intensity * n * 2.0 *con;
			float noise2 = noise - 0.5;
			if (noise2 < 0.0) noise2 = 0.0;

			t = 0.0;// (float)_Time * 0.0 * _Speed;
            float n1 = snoise(float2(0.0, i.texcoord.y * noiseScale * 10), float2(t, t), 2.0, 1.0);
            t = 0.0;//(float)_Time * 10.0 * _Speed;
            float n2 = snoise(float2(0.0, i.texcoord.y * noiseScale * 100), float2(t, t), 3.0, 1.0);
			t = 0.0;//(float)_Time * 100.0 * _Speed;
			float n3 = snoise(float2(0.0, i.texcoord.y * noiseScale * 1000), float2(t, t), 1.0, 1.0);
			n2 -= 0.4;
			if (n2 < 0.0) n2 = 0.0;

			//float2 resultUV =  i.texcoord + float2((n1 + (n2 * n3) - 0.5 - n3 * 0.5), 0.0) * noise;
			
			float2 resultUV = fmod(i.texcoord + float2(n1 * noise + n2 * n3 * noise, 1.0), 1.0);// +float2(((n1 + n2)* noise + (n3 - 0.5 * n2) * noise2), 0.0), 1.0);
            float black = _Intensity * 0.1;
            
			float4 result = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, resultUV);// * (1.0 - noise * 0.5)// + 
				//n2rand(resultUV) * noise * 0.5 - float4(black, black, black, 1.0);
			result.a = 1.0;

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