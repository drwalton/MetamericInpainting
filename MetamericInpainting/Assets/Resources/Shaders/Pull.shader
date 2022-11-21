Shader "Hidden/Pull"
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
	SamplerState my_linear_clamp_sampler;

	float4 _MainTex_TexelSize;

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

		float4 res= _MainTex.SampleLevel(my_point_clamp_sampler, input.uv, _LOD);
		if (res.a < 0.5) {
			return _MainTex.SampleLevel(my_linear_clamp_sampler, input.uv, _LOD +1);
		}

		return _MainTex.SampleLevel(my_linear_clamp_sampler, input.uv, _LOD);
	
		

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
