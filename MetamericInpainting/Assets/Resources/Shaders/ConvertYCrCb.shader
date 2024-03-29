﻿Shader "Hidden/ConvertYCrCb"
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

	float _LOD;
	int _Direction;

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
		float4 color = _MainTex.SampleLevel(sampler_MainTex, input.uv, _LOD);
		if (_Direction == -1)
		{
			return float4(YCrCb2rgb(color.xyz), color.a);
		}
		if(_Direction == 1)
		{
			return float4(rgb2YCrCb(color.xyz),color.a);
		}
		
		else
		{

			return color;
		}

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
