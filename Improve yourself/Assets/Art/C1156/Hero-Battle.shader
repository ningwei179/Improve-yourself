// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "HeroShader/Characters/Battle" 
{
	Properties 
	{
		_Color ("Color", Color) = (0.938,0.234,0.035,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_AlphaDetail("AlphaDetail", 2D) = "gray" {}
		_RimColor("RimColor",Color) = (0.953,0.629,0,1)
		_RimRange("RimRange",Range(0,8)) = 1.0
		_Speed("Speed",Range(0,32)) = 16.0
		_EmissPower("EmissPower",Range(0,32)) = 0.0
		_AlphaRange("AlphaRange",Range(0,1)) = 1.0
	}
	
	SubShader 
	{
		Tags {"Queue"="AlphaTest" "RenderType"="Opaque" "ForceNoShadowCasting" = "True" "IgnoreProjector" = "True"}
		Pass
		{
			Tags{ "LightMode" = "Always" }
			Fog{ Mode Off }
			Lighting Off
			Cull Back
			ZWrite On
			ColorMask 0
		}
		Pass
		{
			Tags{"LightMode" = "Always"}
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			Fog{Mode Off}
			Lighting Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			//#pragma target 3.0

			uniform sampler2D _MainTex;
			uniform sampler2D _AlphaDetail;
			float4 _Color;
			float _EmissPower;
			float4 _MainTex_ST;
			float4 _AlphaDetail_ST;
			float _Speed;
			float _RimRange;
			float4 _RimColor;
			float _AlphaRange;

			struct v2f
			{
				float4 pos:SV_POSITION;
				float3 worldNormal:Normal;
				float4 uv0:TEXCOORD0;
				float3 viewDir:TEXCOORD1;
			};

			v2f vert(appdata_tan v)
			{
				//_battleAODir = float3(-11.7, 9.4, -9.8);
				v2f o;
				TANGENT_SPACE_ROTATION;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = (v.normal);
				o.uv0.xy = (v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				o.uv0.zw = (v.texcoord.xy * _AlphaDetail_ST.xy) + _AlphaDetail_ST.zw;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.viewDir = normalize(mul((float3x3)unity_WorldToObject, (_WorldSpaceCameraPos - worldPos.xyz)));
				return o;
			}

			float4 frag(v2f o) :SV_Target
			{
				o.worldNormal = normalize(o.worldNormal);
				float2 uv_NoiseTex = o.uv0.zw*float2(2.0,2.0);
				//uv_NoiseTex.x = o.uv0.z - _Time.y*_Speed;
				uv_NoiseTex.y = o.uv0.w - _Time.x*_Speed;
				float3 noise = tex2D(_AlphaDetail, uv_NoiseTex);				
				float3 col = tex2D(_MainTex, o.uv0.xy).rgb;
				float rim = 1 - saturate(dot(o.worldNormal, o.viewDir));
				float4 c;
				c.rgb = col + noise*_Color.xyz*_EmissPower + (rim*rim*_RimRange)*_RimColor.xyz;
				c.a = _AlphaRange;
				return c;
			}
			ENDCG
		}
	}
}
