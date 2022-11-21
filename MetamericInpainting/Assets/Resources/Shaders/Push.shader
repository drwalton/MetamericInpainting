Shader "Hidden/Push"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#include "UnityCG.cginc"
#include "Utils.cginc"

	Texture2D _MainTex;
	SamplerState sampler_MainTex;

	float4 _MainTex_TexelSize;

	int _LOD;
	int _size;
	float _threshold;
	int _do2x2;

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

	Varyings vertex(in Input input)
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

	float4 fragment(in Varyings input) : SV_Target
	{
		int nValid = 0;
		float4 res = float4(0, 0, 0, 0);
		float minDepth = 100000000;

		if (_do2x2 == 1) {
			//sample
			float indices[2] = {0.5,-0.5};
			float4 colors[4];
			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 2; j++) {
					colors[(i * 2) + j] = _MainTex.SampleLevel(sampler_MainTex, input.uv + float2(indices[i] / _size, indices[j] / _size), _LOD);
				}
			}

			//decode alpha
			bool valid[4];
			for (int i = 0; i < 4; i++) {
				valid[i] = colors[i].a >= 0.5;
			}


			float depths[4];
			for (int i = 0; i < 4; i++) {
				depths[i] = valid[i] ? (colors[i].a - 0.5) * 2 : colors[i].a * 2;
			}

			for (int i = 0; i < 4; i++) {
					minDepth = min(minDepth,depths[i]);
			}

			for (int i = 0; i < 4; i++) {
				if (valid[i] && depths[i] - minDepth < _threshold) {
					nValid++;
					res += colors[i];
				}
			}
		}
		else 
		{
			//sample
			float indices[4] = { 0.5,-0.5,1.5,-1.5 };
			float4 colors[16];
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					colors[(i * 4) + j] = _MainTex.SampleLevel(sampler_MainTex, input.uv + float2(indices[i] / _size, indices[j] / _size), _LOD);
				}
			}

			//decode alpha
			bool valid[16];
			for (int i = 0; i < 16; i++) {
				valid[i] = colors[i].a >= 0.5;
			}


			float depths[16];
			for (int i = 0; i < 16; i++) {
				depths[i] = valid[i] ? (colors[i].a - 0.5) * 2 : colors[i].a * 2;
			}

			for (int i = 0; i < 16; i++) {
				minDepth = min(minDepth, depths[i]);
			}

			for (int i = 0; i < 16; i++) {
				if (valid[i] && depths[i] - minDepth < _threshold) {
					nValid++;
					res += colors[i];
				}
			}
		}
		if (nValid > 0)
			res /= nValid;

		res.a = (minDepth / 2) + (nValid == 0 ? 0 : 0.5);

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
