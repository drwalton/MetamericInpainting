Shader "Hidden/AddN"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

		CGINCLUDE
#include "UnityCG.cginc"
#include "Utils.cginc"

	SamplerState my_trilinear_clamp_sampler;

	Texture2D _Tex1;
	//SamplerState sampler_Tex1;
	Texture2D _Tex2;
	//SamplerState sampler_Tex2;
	Texture2D _Tex3;
	//SamplerState sampler_Tex3;
	Texture2D _Tex4;
	//SamplerState sampler_Tex4;
	Texture2D _Tex5;
	//SamplerState sampler_Tex5;
	Texture2D _Tex6;
	//SamplerState sampler_Tex6;
	Texture2D _Tex7;
	//SamplerState sampler_Tex7;
	Texture2D _Tex8;
	//SamplerState sampler_Tex8;
	Texture2D _Tex9;
	//SamplerState sampler_Tex9;
	Texture2D _Tex10;
	//SamplerState sampler_Tex10;


	float4 _Tex1_TexelSize;

	int _NTextures;


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
		if (_Tex1_TexelSize.y < 0)
			output.uv.y = 1. - input.uv.y;
#endif

		return output;
	}

	float4 fragment(in Varyings input) : SV_Target
	{

		float3 res = float3(0,0,0);
		int n = 0;

		res = res + _Tex1.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures ) return float4(res, 1.0);

		res = res + _Tex2.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex3.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex4.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex5.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex6.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex7.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex8.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex9.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		n = n + 1;
		if (n == _NTextures) return float4(res, 1.0);

		res = res + _Tex10.SampleLevel(my_trilinear_clamp_sampler, input.uv, 0).xyz;
		return float4(res, 1.0);

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
