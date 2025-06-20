Shader "w0w/SH_CRT_Pattern_1"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            // インスペクタに表示したときにわかりやすいように名前を付けておく
            Name "Update"

            CGPROGRAM
            
            // UnityCustomRenderTexture.cgincをインクルードする
           #include "UnityCustomRenderTexture.cginc"

            // 頂点シェーダは決まったものを使う
           #pragma vertex CustomRenderTextureVertexShader
           #pragma fragment frag



           float2 rotateCoord(float2 uv, float rads) {
                return mul( float2x2(cos(rads), sin(rads), -sin(rads), cos(rads)),  uv);
	            //return uv;
            }

            // v2f構造体は決まったものを使う
            half4 frag(v2f_customrendertexture i) : SV_Target
            {
                //return half4(_SinTime.z * 0.5 + 0.5, _CosTime.w * 0.5 + 0.5, _SinTime.w * 0.5 + 0.5, 1);

                    // update layout params
                float rows = 10 * 0.5;//linesRows + 3. * sin(iTime);
                float curThickness = 0.25 + 0.2 * cos(_Time*10);
  	            float curRotation = _Time*2;//0.8 * sin(_Time*10);
    // get original coordinate, translate & rotate
	            float2 uv = i.globalTexcoord*2.0-1.0;//(2. * fragCoord - iResolution.xy) / iResolution.y;
    //uv += curCenter;
                uv = rotateCoord(uv, curRotation);
    // create grid coords
                float2 uvRepeat = frac(uv * rows);		
    // adaptive antialiasing, draw, invert
                float aa = 100 * 0.00003; 	
                float col = smoothstep(curThickness - aa, curThickness + aa, length(uvRepeat.y - 0.5));
                //if(invert == 1) col = 1. - col;			
	            return float4(float3(col,col,col), 1.0);


            }

            ENDCG
        }
    }
}

