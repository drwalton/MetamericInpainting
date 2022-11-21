Shader "Hidden/WarpImageMeshMotionInpaint"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        //_MotionTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

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
            };

            struct fout
            {
                float4 col: COLOR0;
                float4 alpha: COLOR1;
                float4 motion: COLOR2;
            };

            Texture2D _MotionTex;
            Texture2D _DepthTex;
            SamplerState my_point_clamp_sampler;

            uniform float warpMultiple;
            v2g vert (appdata v)
            {
                v2g o;
                //float4 motion = tex2Dlod(_MotionTex, float4(v.uv, 0, 0));
                float4 motion = _MotionTex.SampleLevel(my_point_clamp_sampler, v.uv, 0);

                //float4 depth = tex2Dlod(_DepthTex, float4(v.uv, 0, 0));
                float4 depth = _DepthTex.SampleLevel(my_point_clamp_sampler, v.uv, 0);
                o.vertex = float4(
                    v.vertex.x + motion.x * warpMultiple * 2, // Note the *2 here is because clip space is [-1,1]
                    v.vertex.y - motion.y * warpMultiple * 2,
                    depth.x,
                    1.0);
                if (v.vertex.z > 0.5) {
                    o.vertex = float4(v.vertex.x, v.vertex.y, 1.0, 1.0);
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
                float2 warpedUV = input[0].uv;
                float minDepth = input[0].vertex.z;

                if (input[1].vertex.z < input[0].vertex.z) 
                {
                    minDepth = input[1].vertex.z;
                    warpedUV = input[1].uv;
                }
                if (input[2].vertex.z < input[1].vertex.z) 
                {
                    minDepth = input[2].vertex.z;
                    warpedUV = input[2].uv;
                }
                
                minDepth = minDepth * 0.5;


                output.vertex = input[0].vertex;
                if (valid) {
                    output.vertex.z = (output.vertex.z * 0.5) + 0.5;
                    output.uv = input[0].uv;
                }
                else {
                    output.vertex.z = minDepth;
                    output.uv = warpedUV;
                }
                triStream.Append(output);
                output.vertex = input[1].vertex;
                if (valid) {
                    output.uv = input[1].uv;
                    output.vertex.z = (output.vertex.z * 0.5) + 0.5;
                }
                else {
                    output.vertex.z = minDepth;
                    output.uv = warpedUV;

                }
                triStream.Append(output);
                output.vertex = input[2].vertex;
                
                if (valid) {
                    output.uv = input[2].uv;
                    output.vertex.z = (output.vertex.z * 0.5) + 0.5;
                }
                else {
                    output.vertex.z = minDepth;
                    output.uv = warpedUV;

                }
                triStream.Append(output);
                triStream.RestartStrip();
            }

            Texture2D _MainTex;

            fout frag(g2f i) : SV_Target
            {
                //float4 col = float4(tex2D(_MainTex, i.uv));
                fout res;
                res.col = _MainTex.Sample(my_point_clamp_sampler, i.uv);
                res.col.a = i.vertex.z;
                res.alpha = float4(res.col.a, res.col.a, res.col.a, res.col.a);
                res.motion = _MotionTex.Sample(my_point_clamp_sampler, i.uv);
                return res;
            }
            ENDCG
        }
    }
}
