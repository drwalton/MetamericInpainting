Shader "Hidden/ConvolutionMasked"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Utils.cginc"

	Texture2D _MainTex;
	SamplerState sampler_MainTex;

	float4 _MainTex_TexelSize;

	float2 _Direction;


	float _MeanDepth = 3;
	float _FoveaSize = 0.1;
	//float _FoveaX = 0.5;
	//float _FoveaY = 0.5;

#define MAX_KERNEL_SIZE 1000

	float _Kernel[MAX_KERNEL_SIZE];
	int _KernelWidth;
	float2 _TexelSize;
	float _K = 1;
	float _LOD;

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


		float3 value = float3(0,0,0);
		float spread = (_KernelWidth -1) / 2.0;
		int k = 0;
		float centerAlpha = 0;
		bool isAnyInvalid = false;
		float maxAlpha = -1;
		for (float i = -spread; i <= spread; i++) 
		{
			for (float j = -spread; j <= spread; j++)
			{
				float4 sampled = _MainTex.SampleLevel(sampler_MainTex, input.uv + float2(i, j) * _TexelSize, _LOD);
				float4 sampledOG = _MainTex.SampleLevel(sampler_MainTex, input.uv + float2(i, j) * _TexelSize, 0);

				if (sampledOG.a < 0.5)
					isAnyInvalid = true;
				if (i == 0 && j == 0)
					centerAlpha = sampledOG.a;
				maxAlpha = max(sampledOG.a, maxAlpha);
				value = value + sampled.xyz * _Kernel[k] *_K;
				k = k + 1;
			}
		}

		if (isAnyInvalid) maxAlpha = centerAlpha;
		if (isAnyInvalid && maxAlpha >= 0.5) maxAlpha -= 0.5;

		return float4(value,maxAlpha);
			
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
