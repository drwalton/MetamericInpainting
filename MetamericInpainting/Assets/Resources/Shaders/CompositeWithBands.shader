Shader "Hidden/CompositeWithBands"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#include "UnityCG.cginc"
#include "Utils.cginc"

		Texture2D _MainTex;
	SamplerState my_point_clamp_sampler;

	Texture2D _InpaintedTex;
	SamplerState sampler_InpaintedTex;

	Texture2D _InpaintedBands;
	SamplerState sampler_InpaintedBands;

	float4 _MainTex_TexelSize;

	float _FoveaSize;
	float _FoveaX;
	float _FoveaY;
	float _MeanDepth;

	int _Blend;
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

		float4 warped = _MainTex.SampleLevel(my_point_clamp_sampler, input.uv, 0);

		float4 inpainted = _InpaintedTex.SampleLevel(sampler_InpaintedTex, input.uv, 0);
		float4 inpaintedBands = _InpaintedBands.SampleLevel(sampler_InpaintedBands, input.uv, 0);

		if (warped.a < 0.5)
			return inpainted + inpaintedBands;
		else
			return inpainted;
	

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
