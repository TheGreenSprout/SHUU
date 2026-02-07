/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



Shader "Debug/GLDebug"
{
    Properties
    {
        _ZTest("ZTest Mode", Int) = 8 // Default to LessEqual
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Overlay" }
        Pass
        {
            ZWrite Off
            ZTest [_ZTest]        // ← dynamically controlled from C#
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
