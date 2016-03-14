Shader "NewChromantics/Outline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		OutlineSize("OutlineSize", Range(0,1) ) = 0.05
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float OutlineSize;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float w = OutlineSize / 2.f;
				//	discard if edges
				if ( i.uv.x > w && 
					i.uv.x < 1.f-w &&
					i.uv.y > w &&
					i.uv.y < 1.f-w )
				{
					discard;
				}

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
