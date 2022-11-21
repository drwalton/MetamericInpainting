
Shader "Hidden/WarpImageMeshEqui"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        //_MotionTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Uncomment the #define below to add black wireframe lines along triangle edges.
            //#define WIREFRAME
            // Using the below #define, we test for validity based on the depth values of triangle verts, taken from the 
            // supplied depth video.
            // If this isn't defined, the test looks at the maximum side length of the triangle.
            #define DEPTH_VALID_TEST

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float depth : DEPTH;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float depth : DEPTH;
#ifdef WIREFRAME
                float3 bary : BARY;
#endif
            };

            Texture2D _DepthTex;
            SamplerState my_point_clamp_sampler;
            float4x4 warpMatrix;
            float depthScale;

            uniform float warpMultiple;
            v2g vert (appdata v)
            {
                v2g o;
                float4 depth = _DepthTex.SampleLevel(my_point_clamp_sampler, v.uv, 0);
                o.uv = v.uv;
                o.vertex = v.vertex;
                o.vertex.w = 1.0f;
                depth.x = max(depth.x, 0.02f);
                o.vertex.xyz *= depthScale / depth.x;

                o.vertex = mul(warpMatrix, o.vertex);

                //o.vertex.xyz /= o.vertex.w;
                //o.vertex.w = 1.0;

                //o.vertex.z = (o.vertex.z + 1) * 0.5;

                //o.depth = clamp(o.vertex.z, 0, 1);
                o.depth = depth.x;
                return o;
            }

            uniform float triangleValidThreshold;

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                g2f output;
                float3 v0 = input[0].vertex.xyz / input[0].vertex.w;
                float3 v1 = input[1].vertex.xyz / input[1].vertex.w;
                float3 v2 = input[2].vertex.xyz / input[2].vertex.w;
                //Check triangle and mark as invalid if too big
#ifdef DEPTH_VALID_TEST
                float maxD = max(input[0].depth, max(input[1].depth, input[2].depth));
                float minD = min(input[0].depth, min(input[1].depth, input[2].depth));
                bool valid = (maxD - minD) <= triangleValidThreshold;
#else
                float maxSideLen =
                    max(length(v0 - v1), max(length(v0 - v2), length(v1 - v2)));
                bool valid = maxSideLen <= triangleValidThreshold;
#endif
                float minDepth = min(v0.z, min(v1.z, v2.z));
                minDepth = (minDepth) * 0.5;
                //minDepth = 0.12;

                output.uv = input[0].uv;
                output.vertex = input[0].vertex;
                
#ifdef WIREFRAME
                output.bary = float3(1, 0, 0);
#endif
                if (valid) output.depth = (input[0].vertex.z / input[0].vertex.w) * 0.5 + 0.5;
                else output.depth = minDepth;
                triStream.Append(output);

                output.uv = input[1].uv;
                output.vertex = input[1].vertex;
#ifdef WIREFRAME
                output.bary = float3(0, 1, 0);
#endif
                if (valid) output.depth = (input[1].vertex.z / input[1].vertex.w) * 0.5 + 0.5;
                else output.depth = minDepth;
                triStream.Append(output);
                
                output.uv = input[2].uv;
                output.vertex = input[2].vertex;
#ifdef WIREFRAME
                output.bary = float3(0, 0, 1);
#endif
                if (valid) output.depth = (input[2].vertex.z / input[2].vertex.w) * 0.5 + 0.5;
                else output.depth = minDepth;
                triStream.Append(output);

                triStream.RestartStrip();
            }

            Texture2D _MainTex;

            float4 frag(g2f i) : SV_Target
            {
                float4 col = _MainTex.Sample(my_point_clamp_sampler, i.uv);
#ifdef WIREFRAME
                float minBary = min(i.bary.x, min(i.bary.y, i.bary.z));
                minBary = smoothstep(0, 0.1, minBary);
                col *= minBary;
#endif
                col.a = clamp(i.depth, 1e-6, 1 - (1e-6));
                return col;
            }
            ENDCG
        }
    }
}
