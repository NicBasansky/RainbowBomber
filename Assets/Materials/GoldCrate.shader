Shader "Custom/GoldCrate"
{
    Properties
    {
        _Color ("Colour", Color) = (1, 1, 1, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        //_MetallicTex ("Metallic Texture", 2D) = "white" {}
        _Cube ("Cube Map", Cube) = "black" {}
        _SpecColor ("Specular", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.5
    }
    SubShader
    {
        CGPROGRAM
        #pragma surface surf Standard

        fixed4 _Color;
        sampler2D _MainTex;
        //sampler2D _MetallicTex;
        samplerCUBE _Cube;
        float _SpecAmount;
        fixed _Smoothness;
        fixed _Metallic;

        struct Input 
        {
            float2 uv_MainTex;
            float3 worldRefl;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Smoothness = _Smoothness;
            o.Metallic = _Metallic.r;
            //o.Specular = _SpecColor.rgb;
            //o.Emission = texCUBE(_Cube, IN.uv_MainTex);
        }
        ENDCG
    }
    Fallback "Diffuse"
}