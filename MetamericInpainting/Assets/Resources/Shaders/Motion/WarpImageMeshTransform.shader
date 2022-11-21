Shader "Hidden/WarpImageMeshTransform"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        //_MotionTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            //#define WIREFRAME

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
#ifdef WIREFRAME
                float3 bary : BARY;
#endif
            };

            Texture2D _DepthTex;
            SamplerState my_linear_clamp_sampler;
            SamplerState my_point_clamp_sampler;
            SamplerState my_point_repeat_sampler;

            float4x4 camMatrix;
            float4x4 transformMatrix;
            float4x4 invCamMatrix;

            uniform float warpMultiple;
            uniform float depthMultiple;
            uniform float depthOffset;
            v2g vert (appdata v)
            {
                v2g o;
                float4 depth = depthMultiple * _DepthTex.SampleLevel(my_point_clamp_sampler, v.uv, 0) + float4(depthOffset, 0, 0, 0);

                if (v.vertex.z > 0.5) {
                    o.vertex = float4(v.vertex.x, v.vertex.y, 1.0, 1.0);
                }
                else {
					o.vertex = mul(invCamMatrix, float4(
						v.vertex.x,
						v.vertex.y,
						depth.x,
						1.0));
                    o.vertex /= o.vertex.w;
					o.vertex = mul(transformMatrix, o.vertex);
					o.vertex = mul(camMatrix, o.vertex);

                    //o.vertex.z = max(5e-2, o.vertex.z);
                    //o.vertex.z = clamp(o.vertex.z, 5e-2f, 1.0f);
                    o.vertex /= o.vertex.w;
                    //o.vertex.w = 1.0;
                }
                o.uv = v.uv;
                return o;
            }

            uniform float sideLenThreshold;

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                //Check triangle and mark as invalid if too big
                g2f output;
                float maxSideLen = 
                    max(length(input[0].vertex.xy - input[1].vertex.xy),
                        max(length(input[0].vertex.xy - input[2].vertex.xy),
                            length(input[1].vertex.xy - input[2].vertex.xy)));
                bool valid = maxSideLen <= sideLenThreshold;
                float minDepth = min(input[0].vertex.z, min(input[1].vertex.z, input[2].vertex.z));
                minDepth = minDepth * 0.5;


                output.uv = input[0].uv;
                output.vertex = input[0].vertex;
#ifdef WIREFRAME
                output.bary = float3(1, 0, 0);
#endif
                if (valid) output.vertex.z = (output.vertex.z * 0.5) + 0.5;
                else output.vertex.z = minDepth;
                triStream.Append(output);
                output.uv = input[1].uv;
                output.vertex = input[1].vertex;
#ifdef WIREFRAME
                output.bary = float3(0, 1, 0);
#endif
                if (valid) output.vertex.z = (output.vertex.z * 0.5) + 0.5;
                else output.vertex.z = minDepth;
                triStream.Append(output);
                output.uv = input[2].uv;
                output.vertex = input[2].vertex;
#ifdef WIREFRAME
                output.bary = float3(0, 0, 1);
#endif
                if (valid) output.vertex.z = (output.vertex.z * 0.5) + 0.5;
                else output.vertex.z = minDepth;
                triStream.Append(output);
                triStream.RestartStrip();
            }

            Texture2D _MainTex;

            float4 frag(g2f i) : SV_Target
            {

                float4 col = _MainTex.Sample(my_point_clamp_sampler, i.uv);
                
                col.a = clamp(i.vertex.z, 1e-6, 1 - (1e-6));
                //col.a = i.vertex.z;
#ifdef WIREFRAME
                float minBary = min(i.bary.x, min(i.bary.y, i.bary.z));
                minBary = smoothstep(0, 0.1, minBary);
                col *= minBary;
#endif
                return col;

            }
            ENDCG
        }
    }
}
