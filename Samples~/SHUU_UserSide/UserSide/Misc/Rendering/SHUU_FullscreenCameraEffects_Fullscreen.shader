/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



Shader "Custom/SHUU_FullscreenCameraEffects_Fullscreen"
{
    Properties
{
    [Toggle] _EnablePixelate ("Enable Pixelation", Float) = 0
    [Toggle] _EnableColorRes ("Enable Color Resolution", Float) = 0
    [Toggle] _EnableDither ("Enable Dither", Float) = 0
    [Toggle] _EnableBW ("Enable Black & White", Float) = 0

    _PixelBlockSize("Pixel Block Size", Float) = 8.0
    [Range(2,64)] _ColorResolution ("Color Resolution", Float) = 8
    _DitherTex ("Dither Texture", 2D) = "white" {}
    [Range(0,1)] _DitherStrength ("Dither Strength", Float) = 0.1
    [Range(1,32)] _DitherScale ("Dither Scale", Float) = 8
}

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "FullscreenPass"
            Tags { "LightMode"="UniversalForward" }

            Cull Off ZWrite Off ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct FSInput { uint vertexID : SV_VertexID; };
            struct FSOutput { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            FSOutput Vert(FSInput input)
            {
                FSOutput o;
                o.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return o;
            }

            // Textures
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(_DitherTex);
            SAMPLER(sampler_DitherTex);

            // Parameters
            float _EnablePixelate;
            float _EnableColorRes;
            float _EnableDither;
            float _EnableBW;

            float _PixelBlockSize;
            float _ColorResolution;
            float _DitherStrength;
            float _DitherScale;

            float4 Frag(FSOutput i) : SV_Target
            {
                float2 uv = i.uv;

                // --- Pixelation ---
                if (_EnablePixelate > 0.5)
                {
                    float2 pixelPos = i.uv * _ScreenParams.xy;
                    pixelPos = floor(pixelPos / _PixelBlockSize) * _PixelBlockSize;
                    uv = (pixelPos + (_PixelBlockSize * 0.5)) / _ScreenParams.xy;
                }

                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

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
                    float2 duv = (i.uv * _ScreenParams.xy) / _DitherScale;
                    float3 dither = SAMPLE_TEXTURE2D(_DitherTex, sampler_DitherTex, duv).rgb - 0.5;
                    col.rgb += dither * _DitherStrength;
                }

                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
