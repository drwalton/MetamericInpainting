 Shader "Hidden/MotionShaderWarp" 
 {
	 SubShader 
	 {
		 Tags { "RenderType"="Opaque" }
		 
		 Pass
		 {
			 CGPROGRAM
			 #pragma vertex vert
			 #pragma fragment frag
			 #include "UnityCG.cginc"
			 
			 Texture2D _CameraMotionVectorsTexture;
			 Texture2D currentMotion;
			 SamplerState my_point_clamp_sampler;
			 SamplerState my_linear_clamp_sampler;
			 
			 struct v2f 
			 {
			    float4 pos : SV_POSITION;
			    float4 scrPos:TEXCOORD1;
			 };
			 
			 //Vertex Shader
			 v2f vert (appdata_base v)
			 {
			    v2f o;
			    o.pos = UnityObjectToClipPos (v.vertex);
			    o.scrPos=ComputeScreenPos(o.pos);
			    return o;
			 }
			 
			 //Fragment Shader
			 float4 frag(v2f i) : COLOR
			 {
				float2 screenPos = i.scrPos;
				//float4 currMotion = currentMotion.Sample(my_linear_clamp_sampler, screenPos);
				//float4 newMotion = _CameraMotionVectorsTexture.Sample(my_linear_clamp_sampler, screenPos + currMotion.xy);
				float4 currMotion = currentMotion.Sample(my_point_clamp_sampler, screenPos);
				float4 newMotion = _CameraMotionVectorsTexture.Sample(my_point_clamp_sampler, screenPos + currMotion.xy);
				return currMotion + newMotion;
			 }
			 ENDCG
		 }
	 }
	 FallBack "Diffuse"
 }