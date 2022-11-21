Shader "Hidden/Viewer"
{
    Properties
    {
       _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"
	#include "..\Utils.cginc"

    struct Input
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    Texture2D _MainTex;
    Texture2D _motionTexture;
    Texture2D _inputColorTexture;
    SamplerState my_point_clamp_sampler;
    SamplerState my_trilinear_clamp_sampler;

    float4 _MainTex_TexelSize;

    float _LOD;
	int _DisplayMode;

	

    bool _pointSample;

    // Comment this to get magenta disocclusions in mode 7
    // Otherwise a gray/white checkerboard will be used.
    #define INVALID_CHECKERBOARD

    Varyings vertex(Input input)
    {
        Varyings output;

        output.vertex = UnityObjectToClipPos(input.vertex.xyz);
        output.uv = input.uv;
       

    #if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
            output.uv.y = 1. - input.uv.y;
    #endif

        return output;
    }

    float3 hsv_2_rgb(float3 c) {
        float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
        float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
        return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
    }

    float4 checkerboard(float2 uv, int nSquaresU, int nSquaresV) {
        int uMod2 = floor(uv.x * nSquaresU) % 2;
        int vMod2 = floor(uv.y * nSquaresV) % 2;
        float4 color0 = float4(0.6, 0.6, 0.6, 1.0);
        float4 color1 = float4(0.9, 0.9, 0.9, 1.0);
        return ((uMod2 + vMod2) % 2) ? color0 : color1;
    }

    float4 fragment(in Varyings input) : SV_Target
    {
        float4 res;
        if(_pointSample) 
		    res = _MainTex.SampleLevel(my_point_clamp_sampler, input.uv, _LOD);
        else
		    res = _MainTex.SampleLevel(my_trilinear_clamp_sampler, input.uv, _LOD);
        
        if (_DisplayMode == 1)
		{
            //if (res.a < 0.5)
                //return float4(0, 0, 0, 1);
			return 20*abs(res);
		}
		if (_DisplayMode == 2)
		{
			return float4((res.xyz*.5) + 0.5,1);
		}
		if (_DisplayMode == 3)
		{
			float4 rgb = float4(YCrCb2rgb(res.xyz), 1);
			return rgb;
		}
		if (_DisplayMode == 4) { //converts from RGB 
			
			float4 col = float4(rgb2YCrCb(res.xyz), 1);
			return col;
		}
        if (_DisplayMode == 5) { //Show alphas
           /* if (res.a < 0.5)
                return float4(1, 0, 1, 1);
            return float4(10*abs(res.xyz), 1);*/
            return float4(res.a, res.a, res.a, 1);
        }
        if (_DisplayMode == 6) { //squared

            return float4(res.xyz* res.xyz, 1);

        }
        if (_DisplayMode == 7) { //normal but with invalid magenta  
            if (res.a < 0.5)
#ifdef INVALID_CHECKERBOARD
                return checkerboard(input.uv, 40, 40);
#else
                return float4(1, 0, 1, 1);
#endif
            return float4(res.xyz, 1);
            //return float4(res.a, res.a, res.a, 1);
        }
        if (_DisplayMode == 8) { //Show depths
            if (res.a < 0.5) {
                res.a += 0.5;
            }
            float alpha = (res.a - 0.5) * 2.0;
            return float4(alpha, alpha, alpha, 1);
        }
        if (_DisplayMode == 9 || _DisplayMode == 10) { // Show motion
            float4 motion;
			if(_pointSample) 
				motion = _motionTexture.SampleLevel(my_point_clamp_sampler, input.uv, _LOD);
			else
				motion = _motionTexture.SampleLevel(my_trilinear_clamp_sampler, input.uv, _LOD);

            if (_DisplayMode == 9) { // Motion direction and magnitude
                const float TWO_PI = 6.28318530718f;
                float angle = atan2(motion.y, motion.x);
                float hue = (angle / TWO_PI) + 0.5f;
                float saturation = length(motion.xy) * 10;
                float value = 1.0f;

                motion.rgb = hsv_2_rgb(float3(hue, saturation, value));
                motion.a = 1.0f;
                res = motion;
            }

            else if (_DisplayMode == 10) { // Motion magnitude and input brightness
                float4 color;
				if(_pointSample) 
					color = _inputColorTexture.SampleLevel(my_point_clamp_sampler, input.uv, _LOD);
				else
					color = _inputColorTexture.SampleLevel(my_trilinear_clamp_sampler, input.uv, _LOD);
                res.rgb = float3(1, 1, 1) * length(color.xyz) * 0.5f;
                res.r += length(motion.xy) * 300;
            }
        }
 

		return res;
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            ENDCG
        }
    }
}
