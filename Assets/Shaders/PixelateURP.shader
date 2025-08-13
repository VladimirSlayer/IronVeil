Shader "Custom/PixelateURP"
{
    Properties
    {
        _PixelSize ("Pixel Size", Float) = 4
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            Name "PixelateURP"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _PixelSize;

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

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 pixelUV = floor(i.uv / (_PixelSize * _MainTex_TexelSize.xy)) * (_PixelSize * _MainTex_TexelSize.xy);
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelUV);
            }

            ENDHLSL
        }
    }
}
