Shader "w0w/SH_Blend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTex ("Color1", 2D) = "white" {}
        _ColorTex2 ("Color2", 2D) = "white" {}
        _ColorTex3 ("Color3", 2D) = "white" {}
        _Color3Mask ("Color3Mask", 2D) = "white" {}
        _PhongTex ("Surface", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _EdgeTex ("Edge", 2D) = "white" {}
        _EdgeMaskTex ("EdgeMask", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _ColorTex;
            sampler2D _ColorTex2;
            sampler2D _ColorTex3;
            sampler2D _Color3Mask;
            sampler2D _PhongTex;
            sampler2D _MaskTex;
            sampler2D _EdgeTex;
            sampler2D _EdgeMaskTex;

            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float random (fixed2 p) 
            { 
                return frac(sin(dot(p+_Time*0.00001, fixed2(12.9898,78.233))) * 43758.5453);
            }

            float rand(float2 co) 
            {
                float a = frac(dot(co+_Time*0.01, float2(2.067390879775102, 12.451168662908249))) - 0.5;
                float s = a * (6.182785114200511 + a * a * (-38.026512460676566 + a * a * 53.392573080032137));
                float t = frac(s * 43758.5453);
                return t;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MaskTex, i.uv);
                fixed4 mask = float4(m,m,m,m);
                fixed4 light = tex2D(_MainTex, i.uv);

                float2 pivot_uv = float2(0.5, 0.5);
                float2 r = (i.uv - pivot_uv) * (1 / 1);
                float2 edgeTexcoord = r + pivot_uv;
                fixed4 edgeMask = tex2D(_EdgeMaskTex, edgeTexcoord);
                fixed4 edge = tex2D(_EdgeTex, i.uv);
                fixed4 color = tex2D(_ColorTex, i.uv);
                fixed4 color2 = tex2D(_ColorTex2, i.uv);
                fixed4 color3 = tex2D(_ColorTex3, i.uv);
                fixed color3Mask = tex2D(_Color3Mask, i.uv);
                fixed4 surf = tex2D(_PhongTex, i.uv);

                float4 grad = float4(0, 0, 0, 1);
                float4 n = surf - rand(i.uv) * 0.1;
                float4 lightsCenter = (light) * (1 - mask*0.9);
                float4 lightsEdge = (edge) * (1 - mask*0.9) * edgeMask;
                //lightsEdge.a = 1;
                float4 pattern = color3;
                
                float4 c = lerp(color - float4(color3.a, color3.a, color3.a, color3.a) * mask + color3, color2, n) + ((lightsEdge + lightsCenter) - rand(i.uv) * 0.1);   
                c.a = 1;
                return c;//c;//lightsCenter;//c;//saturate(c) + light;//  lightsEdge+lightCenter;
            }
            ENDCG
        }
    }
}
