
Shader "Custom/StandartRoughness" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Albedo ("Albedo", 2D) = "white" {}
		_Normal ("Normal", 2D) = ""
        _Roughness ("Roughness", 2D) = "white"
        _Metallic ("Metallic", 2D) = "black"
		_Occlusion ("Occlusion", 2D) = "white"
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
			
		//#pragma multi_compile __ POTATO_MODE

		sampler2D _MainTex;

		struct Input {
			float2 uv_Albedo;
			float2 uv_Metallic;
			float2 uv_Roughness;
			float2 uv_Normal;
			float2 uv_Occlusion;
		};

		fixed4 _Color;

		sampler2D _Albedo;
		sampler2D _Normal;
		sampler2D _Metallic;
		sampler2D _Roughness;
		sampler2D _Occlusion;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 a = tex2D(_Albedo, IN.uv_Albedo) * _Color;
			o.Albedo = a.rgb;
			o.Alpha = a.a;

			o.Metallic = tex2D(_Metallic, IN.uv_Metallic).r;
			o.Smoothness = 1.0 - tex2D(_Roughness, IN.uv_Roughness).r;
			o.Normal = UnpackNormal( tex2D(_Normal, IN.uv_Normal) );
			o.Occlusion = tex2D(_Occlusion, IN.uv_Occlusion).r;
		}
        ENDCG
    }
    FallBack "Diffuse"
}
