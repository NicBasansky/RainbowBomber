Shader "Custom/CoinShader"
{
    Properties {
        _myColor ("Colour", Color) = (1, 1, 1, 1)
        _myCube ("CubeMap", CUBE) = "white" {}
    }

    SubShader {

        CGPROGRAM
            #pragma surface surf Lambert

            fixed4 _myColor;
            samplerCUBE _myCube;

            struct Input {
                float2 uv_myCube;
                float3 worldRefl;
            };

            void surf (Input IN, inout SurfaceOutput o) {
                o.Albedo = _myColor.rgb;
                o.Emission = texCUBE(_myCube, IN.worldRefl).rgb;
            }
        ENDCG
    }
    Fallback "Diffuse"
}
