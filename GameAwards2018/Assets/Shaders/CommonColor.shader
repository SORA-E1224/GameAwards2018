Shader "Custom/CommonColor" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Density("Density", Range(2,50)) = 30
	}
		SubShader
		{
			Tags
			{
				"RenderType" = "Opaque"
			}
			LOD 200

			CGPROGRAM
			#pragma surface surf Lambert vertex:vert
			#pragma target 4.0

			struct Input {
				float4 vertexColor;
				float2 uv_MainTex;
			};

			float _Density;
			sampler2D _MainTex;

			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.vertexColor = v.color;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * IN.vertexColor;
				float2 c = IN.uv_MainTex *_Density;
				c = floor(c) / 2;
				float checker = frac(c.x + c.y) * 2;
				color *= checker;
				o.Albedo = color;
			}

			ENDCG
		}
			FallBack "Diffuse"
}
