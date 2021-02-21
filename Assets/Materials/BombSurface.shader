Shader "Custom/BombSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _CubeMap ("Cube Map", Cube) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _RimColor ("Rim Colour", Color) = (1, 1, 1, 1)
        _RimStrength ("Rim Strength", Range(0.5, 7)) = 3
        _FlashColor ("Flash Colour", Color) = (1, 1, 1, 1)
        _FlashFreq ("Flash Frequency", Range(0.1, 20)) = 1
        _FlashSpeed ("Flash Speed", Range(0.1, 5)) = 3
    }
    SubShader
    {     
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldRefl;
        };

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _RimColor;
        float _RimStrength;
        samplerCUBE _CubeMap;
        half _FlashSpeed;
        fixed4 _FlashColor;
        fixed _FlashFreq;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float t = _Time * _FlashSpeed;
            float sine = 0.5 * sin(t * _FlashFreq) + 0.5;

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = lerp(c, _FlashColor, sine);

            half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = texCUBE(_CubeMap, IN.worldRefl) * pow(rim, _RimStrength) * _RimColor;
 
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
