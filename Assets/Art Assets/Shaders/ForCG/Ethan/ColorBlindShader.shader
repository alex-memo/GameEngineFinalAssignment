Shader "Custom/ColorBlindShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LUT ("LUT", 2D) = "white" {}
        _Contribution ("Contribution", Range(0, 1)) = 1
    }
    SubShader
    {
        // Disable culling, depth writing, and depth testing for full-screen effects
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Include URP Core HLSL libraries for transformations and texture sampling
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #define COLORS 32.0 // Define number of colors for the LUT
            struct Attributes
            {
                float4 positionOS : POSITION; // Object space position
                float2 uv : TEXCOORD0; // UV coordinates
            };
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION; // Homogeneous clip-space position
            };
            // Declare texture and LUT samplers
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_LUT);
            SAMPLER(sampler_LUT);
            // Define LUT texel size and contribution factor
            float4 _LUT_TexelSize;
            float _Contribution;
            // Vertex Shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Transform from object space to homogeneous clip space using .xyz
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }
            // Fragment Shader
            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the base texture
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float maxColor = COLORS - 1.0;
                // Calculate texel offsets based on LUT texel size
                float halfColX = 0.5 / _LUT_TexelSize.z;
                float halfColY = 0.5 / _LUT_TexelSize.w;
                float threshold = maxColor / COLORS;
                // Calculate LUT lookup coordinates
                float xOffset = halfColX + col.r * threshold / COLORS;
                float yOffset = halfColY + col.g * threshold;
                float cell = floor(col.b * maxColor);
                // Sample the LUT texture
                float2 lutPos = float2(cell / COLORS + xOffset, yOffset);
                half4 gradedCol = SAMPLE_TEXTURE2D(_LUT, sampler_LUT, lutPos);
                // Blend the original texture color with the LUT color based on the contribution factor
                return lerp(col, gradedCol, _Contribution);
            }
            ENDHLSL
        }
    }
}
