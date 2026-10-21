Shader "Custom/SHUU_FullscreenCameraEffects_Fullscreen"
{
    Properties
    {
        [Toggle] _EnablePixelate ("Enable Pixelation", Float) = 0
        [Toggle] _EnableColorRes ("Enable Color Resolution", Float) = 0
        [Toggle] _EnableDither ("Enable Dither", Float) = 0
        [Toggle] _EnableBW ("Enable Black & White", Float) = 0

        _PixelBlockSize("Virtual Pixel Rows", Float) = 270.0
        [Range(2,64)] _ColorResolution ("Color Resolution", Float) = 8
        [Range(0,1)] _DitherStrength ("Dither Strength", Float) = 1.0
    }

    SubShader
    {
        Tags
            { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "FullscreenPass"
            Tags
                { "LightMode"="UniversalForward" }

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

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _EnablePixelate;
            float _EnableColorRes;
            float _EnableDither;
            float _EnableBW;

            float _PixelBlockSize;
            float _ColorResolution;
            float _DitherStrength;

            float Bayer8x8(float2 pos)
            {
                uint2 q = uint2(pos) % 8u;
                uint a = q.y ^ q.x;
                uint b = q.y;
                uint i = (a&1u) | ((b&1u)<<1u) | ((a&2u)<<1u) | ((b&2u)<<2u) | ((a&4u)<<2u) | ((b&4u)<<3u);
                uint r = ((i&1u)<<5u)|((i&2u)<<3u)|((i&4u)<<1u)|((i&8u)>>1u)|((i&16u)>>3u)|((i&32u)>>5u);
                return float(r) / 64.0;
            }

            float4 Frag(FSOutput i) : SV_Target
            {
                float2 uv = i.uv;
                float blockSize = max(_ScreenParams.y / max(_PixelBlockSize, 1.0), 1.0);

                float2 bayerPos = floor(i.positionCS.xy);

                if (_EnablePixelate > 0.5)
                {
                    float2 blockIdx = floor(i.positionCS.xy / blockSize);
                    float2 blockOrigin = blockIdx * blockSize;
                    float2 blockCenter = floor(blockOrigin + blockSize * 0.5) + 0.5;
                    uv = blockCenter / _ScreenParams.xy;
                    bayerPos = blockIdx;
                }

                float4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_BlitTexture, uv, 0);

                if (_EnableBW > 0.5)
                {
                    float g = dot(col.rgb, float3(0.299, 0.587, 0.114));
                    col.rgb = g.xxx;
                }

                if (_EnableDither > 0.5 || _EnableColorRes > 0.5)
                {
                    float numSteps = _EnableColorRes > 0.5 ? _ColorResolution : 256.0;

                    float bayerOffset = 0.0;
                    if (_EnableDither > 0.5)
                    {
                        float bayer = Bayer8x8(bayerPos);
                        bayerOffset = (bayer - 0.5) * _DitherStrength;
                    }

                    col.rgb = floor(col.rgb * numSteps + 0.5 + bayerOffset) / numSteps;
                }

                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
