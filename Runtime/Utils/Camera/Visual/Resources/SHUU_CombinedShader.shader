Shader "Custom/SHUU_CombinedShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // --- Toggles ---
        _EnablePixelate ("Enable Pixelation", Float) = 0
        _EnableColorRes ("Enable Color Resolution", Float) = 0
        _EnableDither ("Enable Dither", Float) = 0
        _EnableBW ("Enable Black & White", Float) = 0

        // --- Pixelation ---
        _BlockCount ("Block Count", Vector) = (160, 90, 0, 0)
        _BlockSize ("Block Size", Vector) = (0.00625, 0.0111, 0, 0)
        _HalfBlockSize ("Half Block Size", Vector) = (0.003125, 0.00555, 0, 0)

        // --- Color Resolution ---
        [Range(2,64)] _ColorResolution ("Color Resolution", Float) = 8

        // --- Dither ---
        _DitherTex ("Dither Texture", 2D) = "white" {}
        [Range(0,1)] _DitherStrength ("Dither Strength", Float) = 0.1
        [Range(1,32)] _DitherScale ("Dither Scale", Float) = 8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Combined"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DitherTex);
            SAMPLER(sampler_DitherTex);

            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            float4 _DitherTex_TexelSize;

            float _EnablePixelate;
            float _EnableColorRes;
            float _EnableDither;
            float _EnableBW;

            float4 _BlockCount;
            float4 _BlockSize;
            float4 _HalfBlockSize;

            float _ColorResolution;
            float _DitherStrength;
            float _DitherScale;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // --- Pixelation (from Hidden/Pixelize)
                if (_EnablePixelate > 0.5)
                {
                    float2 blockPos = floor(uv * _BlockCount.xy);

                    float2 blockCenter = blockPos * _BlockSize.xy + _HalfBlockSize.xy;
                    uv = blockCenter;
                }

                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // --- Black & White ---
                if (_EnableBW > 0.5)
                {
                    float g = dot(col.rgb, float3(0.299, 0.587, 0.114));
                    col.rgb = g.xxx;
                }

                // --- Color Resolution ---
                if (_EnableColorRes > 0.5)
                {
                    col.rgb = floor(col.rgb * _ColorResolution) / _ColorResolution;
                }

                // --- Dither ---
                if (_EnableDither > 0.5)
                {
                    float2 duv = (IN.uv * _ScreenParams.xy) / _DitherScale;
                    float3 dither = SAMPLE_TEXTURE2D(_DitherTex, sampler_DitherTex, duv).rgb - 0.5;
                    col.rgb += dither * _DitherStrength;
                }

                return col;
            }
            ENDHLSL
        }
    }
}
