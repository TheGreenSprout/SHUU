/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



Shader "Debug/ColliderWire"
{
    Properties
    {
        _FillColor ("Fill Color", Color) = (0,1,0,0.15)
        _WireColor ("Wire Color", Color) = (0,1,0,1)
        _WireThickness ("Wire Thickness", Range(0.5,3)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 pos : POSITION;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            float4 _FillColor;
            float4 _WireColor;
            float _WireThickness;

            v2g vert (appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float3 bary[3] = {
                    float3(1,0,0),
                    float3(0,1,0),
                    float3(0,0,1)
                };

                for (int i = 0; i < 3; i++)
                {
                    g2f o;
                    o.pos = IN[i].pos;
                    o.bary = bary[i];
                    triStream.Append(o);
                }
            }

            fixed4 frag (g2f i) : SV_Target
            {
                float minBary = min(min(i.bary.x, i.bary.y), i.bary.z);
                float wire = smoothstep(0.0, _WireThickness * 0.01, minBary);

                fixed4 col = lerp(_WireColor, _FillColor, wire);
                return col;
            }
            ENDCG
        }
    }
}
