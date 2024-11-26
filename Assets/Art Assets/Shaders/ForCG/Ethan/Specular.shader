Shader "CustomRenderTexture/Specular"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (0,1,1,1)
        _MainTex ("Base Texture", 2D) = "White" {}
        _Shininess ("Shininess", Range(0.1,100)) = 16
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes{
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings{
                float4 positonHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _SpecColor;
                float _Shininess;
            CBUFFER_END

            Varyings vert(Attributes IN){
                Varyings OUT;
                OUT.positonHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                float3 worldPOSWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS()-worldPOSWS);
                OUT.uv=IN.uv;
                return OUT;
            }
            float4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 normalWS = normalize(IN.normalWS);
                half3 reflectDir = reflect(-lightDir,normalWS);
                half3 viewDir = normalize(IN.viewDirWS);
                half specFactor =pow(saturate(dot(reflectDir, viewDir)),_Shininess);
                half3 Specular = _SpecColor.rgb*specFactor;
                half3 finalColor = _Color.rgb*texColor.rgb+Specular;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
