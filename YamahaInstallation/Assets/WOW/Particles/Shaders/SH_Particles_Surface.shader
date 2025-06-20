Shader "w0w/SH_Particles_Surface1" 
{
	Properties 
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		//_MinDist("Min Distance", Range(0.1, 50)) = 10
		//_MaxDist("Max Distance", Range(0.1, 50)) = 25
		//_TessFactor("Tessellation", Range(1, 50)) = 10
		_Displacement("Displacement", Range(0, 1.0)) = 0.3
		//_Phong("Phong Strengh", Range(0,1)) = 0.5
	}
	SubShader 
	{
		Tags{ "LightMode" = "ForwardBase" }

		LOD 200

		CGPROGRAM
		#include "UnityCG.cginc"
		#include "Assets/CjLib/Shader/Math/Quaternion.cginc"
		#include "Assets/CjLib/Shader/Noise/RandomNoise.cginc"
		#include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

		#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
		//#include "AutoLight.cginc"
		//#pragma surface surf Standard alpha:auto addshadow fullforwardshadows vertex:vert//tessphong:_Phong
		#pragma surface surf Standard addshadow fullforwardshadows//
		#pragma vertex vert
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup

		struct appdata
		{
			//UNITY_POSITION(pos);
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float3 worldNormal : TEXCOORD1;
    		float3 worldPos : TEXCOORD2;
			float4 lmap : TEXCOORD3;
			UNITY_SHADOW_COORDS(4)
			#ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH
    half3 sh : TEXCOORD6;
    #endif
#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldNormal;
			float3 worldPos;
      		float3 viewDir;
		};


		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			struct Particle 
			{
				float3 velocity;
				float3 position;
				float3 position2;
				float3 scale;
				float3 scale2;
				float4 rotate;
				float4 color;
				float4 color2;
				float4 color3;
				float emission;
				int life;
				int death;
				float unique;
			};

			StructuredBuffer<Particle> particles;

		#endif

		float _Displacement;
		float _Metallic;
		float _Glossiness;
		sampler2D _MainTex;

		void setup()
		{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

				float4x4 scale = { 
					particles[unity_InstanceID].scale.x, 0, 0, 0, 
					0, particles[unity_InstanceID].scale.x, 0, 0,
					0, 0, particles[unity_InstanceID].scale.x, 0,
					0, 0, 0, 1 
				};

				float4x4 position = {
					1, 0, 0, particles[unity_InstanceID].position.x, 
					0, 1, 0, particles[unity_InstanceID].position.y,
					0, 0, 1, particles[unity_InstanceID].position.z, 
					0, 0, 0, 1
				};

				unity_ObjectToWorld = mul(mul(position, quaternion_to_matrix(particles[unity_InstanceID].rotate)), scale);
				
				float3x3 w2oRotation;
				w2oRotation[0] = unity_ObjectToWorld[1].yzx * unity_ObjectToWorld[2].zxy - unity_ObjectToWorld[1].zxy * unity_ObjectToWorld[2].yzx;
				w2oRotation[1] = unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[2].yzx - unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[2].zxy;
				w2oRotation[2] = unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[1].zxy - unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[1].yzx;
				float det = dot(unity_ObjectToWorld[0], w2oRotation[0]);
				w2oRotation = transpose(w2oRotation);
				w2oRotation *= rcp(det);
				float3 w2oPosition = mul(w2oRotation, -unity_ObjectToWorld._14_24_34);

				unity_WorldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
				unity_WorldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
				unity_WorldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
				unity_WorldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);

			#endif
		}
		
			float3 modify(float3 pos, float3 normal, float id, float scale, float unique) 
			{
				float noise = SimplexNoise(pos * 0.4 + rand_range(id, 0, 1000) + _Time * 0.3);
				float3 p = pos + normal * noise * (_Displacement * unique) * scale;
				//* (max(0.5, unique) - 0.5) * 2
				return p;
			}        

		void vert(inout appdata v)
		{
			//UNITY_INITIALIZE_OUTPUT(appdata, o);
			 UNITY_SETUP_INSTANCE_ID(v);
			 //UNITY_TRANSFER_INSTANCE_ID(v, o);
			int id = 0;
			float scale = 0;
			float unique = 0;
			
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				id = unity_InstanceID;
				scale = particles[unity_InstanceID].scale.x * 0.8 + 0.2;
				unique = particles[unity_InstanceID].unique;
			#endif

			float3 pos = modify(v.vertex, v.normal, id, scale, unique);
			float3 tangent = v.tangent;
			float3 binormal = normalize(cross(v.normal, tangent));
			float delta = 0.25;
			float3 posT = modify(v.vertex + tangent * delta, v.normal, id, scale, unique);
			float3 posB = modify(v.vertex + binormal * delta, v.normal, id, scale, unique);
			float3 modifiedTangent = posT - pos;
			float3 modifiedBinormal = posB - pos;

			v.normal = normalize(cross(modifiedTangent, modifiedBinormal));
			v.vertex = float4(pos.xyz, 0);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float emission = 0;
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				emission = particles[unity_InstanceID].emission;  
				if(particles[unity_InstanceID].life <= 0 && particles[unity_InstanceID].death <= 0) clip(-1.0);
			#endif
			
			o.Albedo = float4(1, 1, 1, 1);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;

			float fresnel = dot(IN.worldNormal, IN.viewDir);
			fresnel = saturate(fresnel) * emission;
			o.Emission = float4(fresnel, fresnel, fresnel, fresnel);
		}

		ENDCG
		//FallBack "Diffuse"
	}
}