Shader "Unlit/BoxVolumetricFog"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MaxDistance("Max distance", float) = 100
        _StepSize("StepSize", Range(0.1,20)) = 1 
        _DensityMultiplier("DensityMultiplier", Range(0,10)) = 1
        _NoiseOffset("Noise Offset", float) = 0

        _FogNoise("Fog noise", 3D) = "white" {}
        _NoiseTiling("Noise tiling", float) = 1
        _DensityThreshold("Density threshold", Range(0, 1)) = 0.1
        _LightScattering("LightScattering", Range(0,1)) = 0.2

        [HDR]_LightContribution("LightContribution", Color) = (1,1,1,1)
        _MinBound("Min Bound", Vector) = (-0.5, -0.5, -0.5)
        _MaxBound("Max Bound", Vector) = (0.5, 0.5, 0.5)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _Color;

            float _MaxDistance;
            float _StepSize;
            float _DensityMultiplier;
            float _NoiseOffset;

            TEXTURE3D(_FogNoise);
            float _NoiseTiling;
            float _DensityThreshold;
            float4 _LightContribution;
            float _LightScattering;
            float3 _MinBound;
            float3 _MaxBound;


            float henyey_greenstein(float angle, float scattering)
            {
                return (1.0 - angle * angle) / (4.0 * PI * pow(1.0 + scattering * scattering - (2.0 * scattering) * angle, 1.5f));
            }

            float get_density(float3 worldPos)
            {
                float4 noise = _FogNoise.SampleLevel(sampler_TrilinearRepeat, worldPos * 0.01 * _NoiseTiling, 0);
                float density = dot(noise, noise);
                density = saturate(density - _DensityThreshold) * _DensityMultiplier;
                return density;
            }

            bool is_inside_bounds(float3 pos, float3 minBound, float3 maxBound)
            {
                return all(pos >= minBound && pos <= maxBound);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);
                float3 entryPoint = _WorldSpaceCameraPos;
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                if (!is_inside_bounds(worldPos, _MinBound, _MaxBound))
                {
                    return col; 
                }

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distLimit = min(viewLength, _MaxDistance);
                float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _NoiseOffset;
                float transmittance = 1;
                float4 fogCol = _Color;

                while (distTravelled < distLimit)
                {
                    float3 rayPos = entryPoint + rayDir * distTravelled;
                    if (!is_inside_bounds(rayPos, _MinBound, _MaxBound)) break;

                    float density = get_density(rayPos);
                    if (density > 0)
                    {
                        Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos));
                        fogCol.rgb += mainLight.color.rgb * _LightContribution.rgb * density * mainLight.shadowAttenuation * _StepSize * henyey_greenstein(dot(rayDir, mainLight.direction), _LightScattering);
                        transmittance *= exp(-density * _StepSize);
                    }

                    distTravelled += _StepSize;
                }

                return lerp(col, fogCol, 1.0 - saturate(transmittance));
            }
            ENDHLSL
        }
    }
}
