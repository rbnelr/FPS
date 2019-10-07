Shader "Custom/AddShadowTransparent" {
	Properties
	{
		_Color ("Color", Color) = (0,0,0,1)
		_Albedo ("Albedo", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull back
		LOD 100

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

			fixed4 _Color;
			sampler2D _Albedo;
			float4 _Albedo_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _Albedo);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
				col.a *= tex2D(_Albedo, i.uv).r;
				return col;
		}
		ENDCG
	}
	}
}
