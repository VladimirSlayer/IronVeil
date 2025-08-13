Shader "Custom/GlowThroughWalls"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (1, 1, 0, 1)  
        _GlowIntensity ("Glow Intensity", Float) = 1.5  
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay" }
        LOD 100

        Pass
        {
            Name "Glow"
            Cull Off          
            ZWrite Off        
            ZTest Always      
            Blend SrcAlpha One 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _GlowColor;
            float _GlowIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return _GlowColor * _GlowIntensity;
            }
            ENDHLSL
        }
    }

    FallBack "Unlit/Color"
}