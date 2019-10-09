Shader "Custom/PotatoModeShader" {
    Properties {
		_Color("Color", Color) = (1,1,1,1)
		_Albedo("Albedo", 2D) = "white" {}
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            HLSLPROGRAM
			#pragma target 3.5

            #pragma vertex vert
            #pragma fragment frag
            //#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma multi_compile __ POTATO_MODE

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

			CBUFFER_START(UnityPerFrame)	
				float4x4 unity_MatrixVP;
			CBUFFER_END

			CBUFFER_START(UnityPerDraw)
				float4x4 unity_ObjectToWorld;
			CBUFFER_END

			#define UNITY_MATRIX_M unity_ObjectToWorld

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			
			CBUFFER_START(UnityPerMaterial)
				float4 _Color;

				sampler2D _Albedo;
				float4 _Albedo_ST;
			CBUFFER_END

			#define MAX_VISIBLE_LIGHTS 4
			
			CBUFFER_START(_LightBuffer)
				float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
				float4 _VisibleLightDirections[MAX_VISIBLE_LIGHTS];
			CBUFFER_END
			
			float3 CalcDiffuseLight (int index, float3 normal_world) {
				float3 lightColor = _VisibleLightColors[index].rgb;
				float3 lightDirection = _VisibleLightDirections[index].xyz;
				float diffuse = saturate(dot(normal_world, lightDirection));
				return diffuse * lightColor;
			}

			struct VertIn {
				float4 pos_obj : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal_obj : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertOut {
				float4 pos_clip : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal_world : TEXCOORD1;
			};

			VertOut vert (VertIn i) {
				VertOut o;
				UNITY_SETUP_INSTANCE_ID(i);
				float4 pos_world = mul(UNITY_MATRIX_M, float4(i.pos_obj.xyz, 1.0));
				o.pos_clip = mul(unity_MatrixVP, float4(pos_world.xyz, 1.0));
				o.uv = TRANSFORM_TEX(i.uv, _Albedo);
				o.normal_world = mul((float3x3)UNITY_MATRIX_M, i.normal_obj);
                return o;
            }

            float4 frag (VertOut v) : SV_Target {
				v.normal_world = normalize(v.normal_world);

				float4 albedo = tex2D(_Albedo, v.uv) * _Color;

				//float3 diffuseLight = 0;
				//for (int i=0; i<MAX_VISIBLE_LIGHTS; ++i) {
				//	diffuseLight += CalcDiffuseLight(i, v.normal_world);
				//}

				float3 diffuseLight = saturate(dot(v.normal_world, normalize(float3(1,2,3))));

				float3 color = diffuseLight * albedo.rgb;

				return float4(color, albedo.a);
            }
            ENDHLSL
        }
    }
}
