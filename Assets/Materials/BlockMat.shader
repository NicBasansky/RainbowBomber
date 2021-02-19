Shader "Custom/BlockMat"
{
    Properties 
	{
	    _mainColor ("Main Color: ", Color) = (1, 1, 1, 1)
		_lineColor ("Line Color: ", Color) = (1, 1, 1, 1)
		_lineWidth ("Line Width: ", Range(0, 1)) = 0.1
		_cellSize ("Cell Size: ", Range(0, 100)) = 1
		//_cubeMap ("CubeMap", CUBE) = "black" {}
		_overlay ("Overlay Texture", 2D) = "white" {}
		_scrollX ("Scroll X", Range(-5, 5)) = 0.88
		_scrollY ("Scroll Y", Range(-5, 5)) = 1.1
		_brightness ("Brightness", Range(0, 5)) = 3.32
	}

  	SubShader 
	{
        pass
        {

        }
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		
		CGPROGRAM
		#pragma surface surf Lambert alpha
		//#include "UnityCG.cginc"

		float4 _lineColor;
		float4 _mainColor;
		float _cellSize;
		fixed _lineWidth;
		//samplerCUBE _cubeMap;
		sampler2D _overlay;
		half _scrollX;
		half _scrollY;
		fixed _brightness;

		struct Input {
			float2 uv_MainTex;	
			float3 worldPos;
			//float3 worldRefl;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			// grid lines
			half val1 = step(_lineWidth * 2, frac(IN.worldPos.x / _cellSize) + _lineWidth);
			half val2 = step(_lineWidth * 2, frac(IN.worldPos.z / _cellSize) + _lineWidth);
			fixed val = 1 - (val1 * val2);

			// scrolling with noise
			_scrollX *= _Time;
			_scrollY *= _Time;
			float2 newUV = IN.uv_MainTex + float2(_scrollX, _scrollY);
			float4 noiseOverlay = tex2D(_overlay, newUV);
			

			o.Albedo = lerp(_mainColor, _lineColor, val) * noiseOverlay * _brightness;
			
			//o.Emission = texCUBE(_cubeMap, IN.worldRefl).rgb;
			o.Alpha = (1 - o.Albedo) * noiseOverlay.a;
	    }
		ENDCG
    }
    Fallback "Diffuse"

}