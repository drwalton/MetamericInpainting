Shader "Hidden/PullSmoothDepth"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#include "UnityCG.cginc"
#include "Utils.cginc"

	Texture2D _MainTex;
	Texture2D _Validity;
	SamplerState my_point_clamp_sampler;
	SamplerState my_linear_clamp_sampler;

	float4 _MainTex_TexelSize;

	float _LOD;
	float _ValidityLOD;

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

	float validityPow;

	float4 fragment(in Varyings input) : SV_Target
	{

		float4 hiResSample = _MainTex.SampleLevel(my_point_clamp_sampler, input.uv, _LOD);
		float4 loResSample = _MainTex.SampleLevel(my_linear_clamp_sampler, input.uv, _LOD +1);
		float hiResValidity = _Validity.SampleLevel(my_point_clamp_sampler, input.uv, _ValidityLOD);
		hiResValidity = pow(hiResValidity,validityPow);
		float3 outputColor = (hiResSample.rgb * hiResValidity) + (loResSample.rgb * (1.0 - hiResValidity));
		return float4(outputColor, hiResValidity);
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
