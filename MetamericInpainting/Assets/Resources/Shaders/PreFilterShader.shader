Shader "Hidden/PreFilter"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SquareTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#include "UnityCG.cginc"
#include "Utils.cginc"

	Texture2D _MainTex;
	SamplerState sampler_MainTex;

	Texture2D _SquareTex;
	SamplerState sampler_SquareTex;

	float4 _MainTex_TexelSize;

	float2 _Direction;

	float _FoveaSize;
	float _FoveaX;
	float _FoveaY;
	float _MeanDepth;

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

		float lod = calculateFoveationLOD(input.uv, _MeanDepth, _FoveaSize, float2(_FoveaX, _FoveaY));

		float4 meanI = _MainTex.SampleLevel(sampler_MainTex, input.uv, lod);
		float4 meanIsquare = _SquareTex.SampleLevel(sampler_SquareTex, input.uv, lod);

		float4 squareMeanI = pow(meanI, 2);
		float4 std = meanIsquare - squareMeanI;

		///////STD is Mean(I^2) - Mean(I)^2///////
		if (std.x < 1e-19)
			std.x = 1e-19;
		if (std.y < 1e-19)
			std.y = 1e-19;
		if (std.z < 1e-19)
			std.z = 1e-19;

		std = sqrt(std);

		float4 val = _MainTex.SampleLevel(sampler_MainTex, input.uv, 0);

		float3 filtered = (val.xyz - meanI.xyz)/std.xyz;
		
		return float4(filtered,1.0);

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
