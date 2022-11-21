Shader "Hidden/MatchNoiseMotion"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#include "UnityCG.cginc"
#include "..\Utils.cginc"



		Texture2D _MeanTex;
	SamplerState sampler_MeanTex;

	Texture2D _StdevTex;
	SamplerState sampler_StdevTex;

	Texture2D _NoiseTex;
	SamplerState sampler_NoiseTex;

	Texture2D _MotionTex;
	SamplerState my_point_clamp_sampler;

	float4 _NoiseTex_TexelSize;


	Texture2D _FoveationLUT;
	SamplerState sampler_FoveationLUT;

	int _useLUT;
	float4 _MainTex_TexelSize;
	float _MeanDepth = 3;
	float _FoveaSize = 0.1;
	float _FoveaX = 0.5;
	float _FoveaY = 0.5;
	int _calcStdFromMean;

	int _LOD;

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
		if (_NoiseTex_TexelSize.y < 0)
			output.uv.y = 1. - input.uv.y;
#endif

		return output;
	}

	float _nWarped;

	float4 fragment(in Varyings input) : SV_Target
	{
		float lod = 0;
		float3 res = float3(0, 0, 0);
		float4 motion = _MotionTex.SampleLevel(my_point_clamp_sampler, input.uv, 0);
		
		float2 shiftedUV = input.uv - _nWarped *(motion.xy);
		//uv goes to 0-1
		//sample
		if (_useLUT == 0)
			lod = calculateFoveationLOD(input.uv, _MeanDepth, _FoveaSize, float2(_FoveaX, _FoveaY));
		else
			lod = _FoveationLUT.SampleLevel(sampler_FoveationLUT, input.uv, 0).r;

		float3 noise = _NoiseTex.SampleLevel(sampler_NoiseTex, shiftedUV, _LOD).xyz;
		float3 mean = _MeanTex.SampleLevel(sampler_MeanTex, input.uv, _LOD).xyz;
		float3 meanLower = _MeanTex.SampleLevel(sampler_MeanTex, input.uv, _LOD).xyz;

		float3 std = _StdevTex.SampleLevel(sampler_StdevTex, input.uv, _LOD).xyz;
		lod = max(lod - _LOD, 0);

		if (_calcStdFromMean != 0) {
			static float lodPows[5] = { 2.5, 2.1, 1.7, 1.7, 1.7 };
			float lodPow = lodPows[_LOD];
			res = (noise * std * (pow(lod / 1.2, lodPow))) + mean;
		}
		 else {
		  res = noise * std + mean;
		}

	return float4(res,1);
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
