Shader "Hidden/SideBySideShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth

        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Utils.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D otherTex;

            half4 frag(v2f i) : SV_Target
            {
                half3 col;
                if (i.uv.x < 0.5) {
                    col = YCrCb2rgb(tex2D(_MainTex, float2(i.uv.x*2, i.uv.y)).xyz);
                }
                else {
                    col = tex2D(otherTex, float2((i.uv.x-0.5)*2, i.uv.y)).xyz;
                }
                return half4(col, 1.0);
            }
            ENDCG
        }
    }
}
