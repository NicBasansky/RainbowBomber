Shader "Custom/Retro Wireframe_English" 
{
    Properties 
	{
	    _mainColor ("Main Color: ", Color) = (1, 1, 1, 1)
		_lineColor ("Line Color: ", Color) = (1, 1, 1, 1)
		_lineWidth ("Line Width: ", Range(0, 1)) = 0.1
		_cellSize ("Cell Size: ", Range(0, 100)) = 1
	}

  	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

		float4 _lineColor;
		float4 _mainColor;
		float _cellSize;
		fixed _lineWidth;

		struct Input {float2 uv_MainTex;	float3 worldPos;};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			half val1 = step(_lineWidth * 2, frac(IN.worldPos.x / _cellSize) + _lineWidth);
			half val2 = step(_lineWidth * 2, frac(IN.worldPos.z / _cellSize) + _lineWidth);
			fixed val = 1 - (val1 * val2);
			o.Albedo = lerp(_mainColor, _lineColor, val);
			o.Alpha = 1;
	    }
		ENDCG
	} 
	FallBack "Diffuse"
}