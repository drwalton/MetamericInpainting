Shader "Hidden/PushSmoothDepth"
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
	SamplerState my_point_clamp_sampler;
	Texture2D _Validity;

	float4 _MainTex_TexelSize;

	float _LOD;
	float _ValidityLOD;
	int _size;
	float _threshold;

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

	struct f2a
	{
		float4 colorAlpha : COLOR0;
		float4 validity : COLOR1;
	};

	int do2x2;

	f2a fragment(in Varyings input) : SV_Target
	{
		// 2x2 version
		//sample
		if (do2x2) {
			float indices[2] = {-0.5,0.5};
			float4 colors[4];
			float4 validity[4];

			int i = 0;
			for (i = 0; i < 2; i++) {
				for (int j = 0; j < 2; j++) {
					colors[(i * 2) + j] = _MainTex.SampleLevel(sampler_MainTex, input.uv + float2(indices[i] / _size, indices[j] / _size), _LOD);
					validity[(i * 2) + j] = _Validity.SampleLevel(sampler_MainTex, input.uv + float2(indices[i] / _size, indices[j] / _size), _ValidityLOD);
				}
			}

			float depths[4];
			for (i = 0; i < 4; i++) {
				depths[i] = colors[i].a >= 0.5 ? (colors[i].a - 0.5) * 2 : colors[i].a * 2;
			}

			float minDepth = 100000000;
			for (i = 0; i < 4; i++) {
					minDepth = min(minDepth,depths[i]);
			}
			for (i = 0; i < 4; i++) {
				if (abs(depths[i] - minDepth) > _threshold ) {
					validity[i] = 0.0;
				}
			}

			float3 weightSumColors = float3(0.0,0.0,0.0);
			float sumValid = 0.0;

			for (i = 0; i < 4; ++i) {
				weightSumColors += colors[i].rgb * validity[i].r;
				sumValid += validity[i].r;
			}

			float3 outputColor = weightSumColors / sumValid;
			float outputValidity = sumValid * 0.25;
			float outputAlpha = (minDepth / 2) + (outputValidity < 0.01 ? 0 : 0.5);

			if (sumValid == 0.0) {
				outputValidity = 0.0;
				outputColor = float3(0, 0, 0);
			}

			f2a output;
			output.colorAlpha = float4(outputColor, outputAlpha);
			output.validity = float4(outputValidity, outputValidity, outputValidity, outputValidity);

			return output;
		}
		else {
			//4x4 version
			//sample
			float indices[4] = { -1.5,-0.5,0.5,1.5 };
			float4 colors[16];
			float4 validity[16];

			int i = 0;
			for (i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					colors[(i * 4) + j] = _MainTex.SampleLevel(sampler_MainTex, input.uv + float2(indices[i] / _size, indices[j] / _size), _LOD);
					validity[(i * 4) + j] = _Validity.SampleLevel(sampler_MainTex, input.uv + float2(indices[i] / _size, indices[j] / _size), _ValidityLOD);
				}
			}

			float depths[16];
			for (i = 0; i < 16; i++) {
				depths[i] = colors[i].a >= 0.5 ? (colors[i].a - 0.5) * 2 : colors[i].a * 2;
			}

			float minDepth = 100000000;
			for (i = 0; i < 16; i++) {
				minDepth = min(minDepth, depths[i]);
			}
			for (i = 0; i < 16; i++) {
				if (depths[i] - minDepth > _threshold) {
					validity[i] = 0.0;
				}
			}

			float3 weightSumColors = float3(0.0, 0.0, 0.0);
			float sumValid = 0.0;

			for (i = 0; i < 16; ++i) {
				weightSumColors += colors[i].rgb * validity[i].r;
				sumValid += validity[i].r;
			}

			float3 outputColor = weightSumColors / sumValid;
			float outputValidity = sumValid / 16.0;
			float outputAlpha = (minDepth / 2) + (outputValidity < 0.01 ? 0 : 0.5);

			if (sumValid == 0.0) {
				outputValidity = 0.0;
				outputColor = float3(0, 0, 0);
			}

			f2a output;
			output.colorAlpha = float4(outputColor, outputAlpha);
			output.validity = float4(outputValidity, outputValidity, outputValidity, outputValidity);

			return output;
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
