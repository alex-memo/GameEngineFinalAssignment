Shader "CustomRenderTexture/Spec+Ambi+Breathing"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _SpecColor("Spec Color", Color) = (0,1,1,0)
        _MainTex("InputTex", 2D) = "white" {}
        _Shininess ("Shininess", Range(0.1,100)) = 16
        _Frequancy("Frequency", Range(1,10)) = 1
        _Amplitude("Amplitude", Range(0.1,1)) = 0.1
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
                float _Frequancy;
                float _Amplitude;
            CBUFFER_END

            Varyings vert(Attributes IN){
                Varyings OUT;
                OUT.positonHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                float3 worldPOSWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS()-worldPOSWS);
                OUT.uv=IN.uv;
                float scale = 1.0+sin(_Time.y*_Frequancy)*_Amplitude;
                float3 vert = IN.positionOS;
                vert.xyz *= scale;
                OUT.positonHCS=TransformObjectToHClip(vert);
                return OUT;
            }
            float4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 normalWS = normalize(IN.normalWS);
                half NdotL = saturate(dot(normalWS, lightDir));
                half3 ambientSH = SampleSH(normalWS);
                half3 diffuse = texColor.rgb*_Color.rgb*NdotL;
                half3 reflectDir = reflect(-lightDir,normalWS);
                half3 viewDir = normalize(IN.viewDirWS);
                float scale = _Shininess-sin(_Time.y*_Frequancy);
                half specFactor =pow(saturate(dot(reflectDir, viewDir)),scale);
                half3 Specular = _Color.rgb*specFactor;

                half3 finalColor = diffuse+ambientSH*texColor.rgb*_Color.rgb*_SpecColor.rgb+Specular;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
