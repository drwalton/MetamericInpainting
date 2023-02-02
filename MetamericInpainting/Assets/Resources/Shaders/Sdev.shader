Shader "Hidden/Sdev"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SqrTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#include "UnityCG.cginc"
#include "Utils.cginc"

	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	SamplerState my_trilinear_clamp_sampler;

	Texture2D _SecondaryTex;
	SamplerState sampler_SecondaryTex;


	float4 _MainTex_TexelSize;


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

		float4 meanI = _MainTex.SampleLevel(my_trilinear_clamp_sampler, input.uv, lod);
		float4 meanIsquare = _SecondaryTex.SampleLevel(my_trilinear_clamp_sampler, input.uv, lod);
		
		float4 squareMeanI = pow(meanI, 2);
		///////STD is Mean(I^2) - Mean(I)^2///////
		float3 std = meanIsquare.xyz - squareMeanI.xyz;

		if (std.x < 1e-15)
			std.x = 1e-15;
		if (std.y < 1e-15)
			std.y = 1e-15;
		if (std.z < 1e-15)
			std.z = 1e-15;
	
		std = sqrt(std);
		return float4(std.xyz, meanI.a);

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
