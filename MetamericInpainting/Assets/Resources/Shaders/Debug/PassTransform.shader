Shader "Hidden/PassTransform"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#include "UnityCG.cginc"
#include "..\Utils.cginc"

		Texture2D _MainTex;
	SamplerState sampler_MainTex;

	float4 _MainTex_TexelSize;
	int _direction;
	float _screenWidth;
	float _screenHeight;
	float _texSize;
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
		float sx = _texSize / _screenWidth;
		float sy = _texSize / _screenHeight;

		if (_direction == 1) {
			output.uv = float2(input.uv.x * sx, input.uv.y * sy);
		}
		else {
			output.uv = float2(input.uv.x / sx, input.uv.y / sy);

		}
#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			output.uv.y = 1. - input.uv.y;
#endif

		return output;
	}

	float4 fragment(in Varyings input) : SV_Target
	{
		return _MainTex.SampleLevel(sampler_MainTex, input.uv,_LOD);

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
