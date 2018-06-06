Shader "Custom/CommonColor" {
	Properties{
		_Color("Color",Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_Density("Density", Range(2,50)) = 30
	}
		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}
			LOD 200

			Pass
			{
				ZWrite ON
				ColorMask 0
			}

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows alpha:fade
			#pragma target 4.0

			struct Input {
				float2 uv_MainTex;
			};

			float _Density;
			sampler2D _MainTex;
			float4 _Color;

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				float2 c = IN.uv_MainTex *_Density;
				c = floor(c) / 2;
				float checker = frac(c.x + c.y) * 2;
				color.rgb *= checker;
				o.Albedo = color.rgb;
				o.Alpha = color.a;
			}

			ENDCG
		}
			FallBack "Diffuse"
}
