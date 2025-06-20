Shader "Hidden/Custom/OpticalDistort"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
#include "Assets/CjLib/HLSL/Noise/SimplexNoise2D.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_PrevTex, sampler_PrevTex);
		TEXTURE2D_SAMPLER2D(_MaskTex, sampler_MaskTex);

	float _Intensity;
	float _Scale;
	float _MaskHold;
	float _Offset;
	float _Lamda;

	float4 _MainTex_TexelSize;

	struct VertexOutput
	{
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;

		float2 left_coord : TEXCOORD1;
		float2 right_coord : TEXCOORD2;
		float2 above_coord : TEXCOORD3;
		float2 below_coord : TEXCOORD4;

		float2 lefta_coord : TEXCOORD5;
		float2 righta_coord : TEXCOORD6;
		float2 leftb_coord : TEXCOORD7;
		float2 rightb_coord : TEXCOORD8;
	};

	float gray(float4 n)
	{
		return (n.r + n.g + n.b) / 3.0;
	}

	VertexOutput Vert(AttributesDefault v)
	{
		VertexOutput o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
		o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		float2 texc = o.texcoord;
		float2 d = float2(0.001, 0.001);

		o.left_coord = (float2(texc + float2(-d.x, 0)));
		o.right_coord = (float2(texc + float2(d.x, 0)));
		o.above_coord = (float2(texc + float2(0, d.y)));
		o.below_coord = (float2(texc + float2(0, -d.y)));

		o.lefta_coord = (float2(texc + float2(-d.x, d.x)));
		o.righta_coord = (float2(texc + float2(d.x, d.x)));
		o.leftb_coord = (float2(texc + float2(-d.x, -d.x)));
		o.rightb_coord = (float2(texc + float2(d.x, -d.x)));

		return o;
	}

	float4 Frag0(VertexOutput i) : SV_Target
	{
		float4 coeffs = float4(0.2126, 0.7152, 0.0722, 1.0);

		float4 a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord) * coeffs;
		float brightness = gray(a);
		a = float4(brightness, brightness, brightness, brightness);

		float4 b = SAMPLE_TEXTURE2D(_PrevTex, sampler_PrevTex, i.texcoord)* coeffs;
		brightness = gray(b);
		b = float4(brightness, brightness, brightness, brightness);

		float2 x1 = float2(_Offset, 0.0);
		float2 y1 = float2(0.0, _Offset)* (_MainTex_TexelSize.z / _MainTex_TexelSize.w);
		float2 texcoord0 = i.texcoord;
		float2 texcoord1 = i.texcoord;

		float4 curdif = b - a;
		float4 gradx = SAMPLE_TEXTURE2D(_PrevTex, sampler_PrevTex, texcoord1 + x1) - 
			SAMPLE_TEXTURE2D(_PrevTex, sampler_PrevTex, texcoord1 - x1);
		gradx += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texcoord0 + x1) - 
			SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texcoord0 - x1);

		float4 grady = SAMPLE_TEXTURE2D(_PrevTex, sampler_PrevTex, texcoord1 + y1) - 
			SAMPLE_TEXTURE2D(_PrevTex, sampler_PrevTex, texcoord1 - y1);
		grady += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texcoord0 + y1) - 
			SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texcoord0 - y1);

		float4 gradmag = sqrt((gradx * gradx) + (grady * grady) + float4(_Lamda, _Lamda, _Lamda, _Lamda));

		float4 vx = curdif * (gradx / gradmag);
		float vxd = gray(vx);
		float2 xout = float2(max(vxd, 0.), abs(min(vxd, 0.))) * _Scale;
		float4 vy = curdif * (grady / gradmag);
		float vyd = gray(vy);
		float2 yout = float2(max(vyd, 0.),abs(min(vyd, 0.))) * _Scale;

		float4 mask = saturate(float4(xout.x, xout.y, yout.x, yout.y));

		float4 color = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.texcoord);
		float4 colorL = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.left_coord);
		float4 colorR = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.right_coord);
		float4 colorA = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.above_coord);
		float4 colorB = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.below_coord);

		float4 colorLA = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.lefta_coord);
		float4 colorRA = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.righta_coord);
		float4 colorLB = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.leftb_coord);
		float4 colorRB = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.rightb_coord);

		float4 blurVector = (color + colorL + colorR + colorA + colorB + colorLA + colorRA + colorLB + colorRB) / 9.0;
		
		return mask + _MaskHold * blurVector;
	}

	float4 Frag1(VertexOutput i) : SV_Target
	{
		float2 texcoord0 = i.texcoord;

		float4 color = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.texcoord);
		float4 colorL = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex,  i.left_coord);
		float4 colorR = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.right_coord);
		float4 colorA = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex,  i.above_coord);
		float4 colorB = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex,  i.below_coord);

		float4 colorLA = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex,  i.lefta_coord);
		float4 colorRA = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex,  i.righta_coord);
		float4 colorLB = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex,  i.leftb_coord);
		float4 colorRB = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex,  i.rightb_coord);

		float4 blurVector = (color + colorL + colorR + colorA + colorB + colorLA + colorRA + colorLB + colorRB) / 9.0;
		float2 blurAmount = float2(blurVector.y - blurVector.x, blurVector.w - blurVector.z);
		float4 sample0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MaskTex, clamp(texcoord0 + blurAmount * _Intensity, 0.0, 1.0));
		float4 sample1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MaskTex, clamp(texcoord0 + 1.02 * blurAmount * _Intensity * _Intensity, 0.0, 1.0));
		
		return (sample0 * 3.0 + sample1) / 4.0;

		//return color;
	}

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always

			Pass
		{

			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag0

			ENDHLSL
		}

			Pass
		{

			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag1

			ENDHLSL
		}

	}
}