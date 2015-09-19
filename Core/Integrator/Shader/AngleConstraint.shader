
//Yves Wang @ FISH, 2015, All rights reserved

Shader "ParticlePhysics2D/AngleConstraint" {
	Properties {
		_PositionRT ("_PositionRT", 2D) = "white" {}
		_StateRT ("_StateRT",2D) = "white" {}
		_AngleDeltaRT ("_AngleDeltaRT",2D) = "white" {}
	}
	
	CGINCLUDE
		#include "UnityCG.cginc"
		uniform sampler2D _PositionRT;
		uniform sampler2D _StateRT;
		uniform sampler2D _AngleDeltaRT;
		
		struct appdata_angleVert {
			float4 vertex : POSITION;//vertex.xy = angleRT uv, vertex.z is not used
			float2 uv : TEXCOORD0;//xy = pa-uv
			float4 cl : COLOR0;//xy = pb-uv, zw = pm-uv
		};
		
		struct v2f_angle {
			float4 pos : SV_POSITION;//screen space -1 to 1
			float4 posAnRT : TEXCOORD0;//xy = pa-uv zw = angleRT uv
			float4 posBM : COLOR0;//xy = particle b uv,zw = particle m uv
		};
		
		//rotate point t around point c for radian a
		float2 RotatePoint2D(float2 t, float2 c, float a){
			float cosA = cos(a);
			float sinA = sin(a);
			return float2 (
				(cosA * (t.x - c.x) - sinA * (t.y - c.y) + c.x),
				(sinA * (t.x - c.x) + cosA * (t.y - c.y) + c.y)
			);
			
		}
	ENDCG
		
	SubShader {
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"ForceNoShadowCasting" = "True" 
			"PreviewType"="Plane"
		}
		
		lighting off
		//blend one one
		Zwrite off
		fog {mode off}
		blend off
		ColorMask RG
		
		
		//0
		Pass {
			Name "ParticleA"
			
			
			CGPROGRAM
			#pragma vertex vert_A
			#pragma fragment frag_A
			#pragma target 3.0
			
			v2f_angle vert_A(appdata_angleVert IN){
				v2f_angle OUT;
				OUT.pos = mul ( UNITY_MATRIX_MVP , IN.vertex);
				OUT.pos.xy = IN.uv * 2 -1;
				OUT.posAnRT = float4(IN.uv,IN.vertex.xy);
				OUT.posBM = IN.cl;
				return OUT;
			}
			
			float2 frag_A(v2f_angle i) : SV_Target {
				float delta = tex2D ( _AngleDeltaRT, i.posAnRT.zw);
				float2 posA = tex2D ( _PositionRT , i.posAnRT.xy);
				float2 posM = tex2D ( _PositionRT, i.posBM.zw);
				float2 _posA = RotatePoint2D (posA , posM , delta);
				fixed isFree = tex2D ( _StateRT , i.posAnRT.xy);//pa uv
				//return (_posA - posA) * isFree;
				//return _posA - posA;
				//return float2(0,0);
				return _posA * isFree + posA * (1 - isFree);
			}
			ENDCG
		}
		
		//1
		pass {
			Name "AngleParticleB"
			
			CGPROGRAM
			#pragma vertex vert_B
			#pragma fragment frag_B
			#pragma target 3.0
			
			v2f_angle vert_B (appdata_angleVert IN) {
				v2f_angle OUT;
				OUT.pos = mul ( UNITY_MATRIX_MVP , IN.vertex);
				OUT.pos.xy = IN.cl.xy * 2 -1;
				OUT.posAnRT = float4(IN.uv,IN.vertex.xy);
				OUT.posBM = IN.cl;
				return OUT;
			}
			
			float2 frag_B(v2f_angle i) : SV_Target{
				float delta = tex2D ( _AngleDeltaRT, i.posAnRT.zw);
				float2 posB = tex2D ( _PositionRT , i.posBM.xy);
				float2 posM = tex2D ( _PositionRT, i.posBM.zw);
				float2 _posB = RotatePoint2D (posB , posM , -delta);
				fixed isFree = tex2D ( _StateRT , i.posBM.xy);//pb uv
				//return (_posB - posB) * isFree;
				//return _posB - posB;
				//return float2(0,0);
				return _posB * isFree + posB * (1-isFree);
			}
		
			ENDCG
			
		} 
		
		//2
		pass {
			Name "AngleParticleM"
			
			CGPROGRAM
			#pragma vertex vert_M
			#pragma fragment frag_M
			#pragma target 3.0
			
			v2f_angle vert_M (appdata_angleVert IN) {
				v2f_angle OUT;
				OUT.pos = mul ( UNITY_MATRIX_MVP , IN.vertex);
				OUT.pos.xy = IN.cl.zw * 2 -1;
				OUT.posAnRT = float4(IN.uv,IN.vertex.xy);
				OUT.posBM = IN.cl;
				return OUT;
			}
			
			float2 frag_M(v2f_angle i) : SV_Target{
				float delta = tex2D ( _AngleDeltaRT, i.posAnRT.zw);
				float2 posB = tex2D ( _PositionRT , i.posBM.xy);
				float2 posA = tex2D ( _PositionRT , i.posAnRT.xy);
				float2 posM = tex2D ( _PositionRT, i.posBM.zw);
				float2 _posM = RotatePoint2D (posM , posA , delta);
				_posM = RotatePoint2D(_posM,posB,-delta);
				fixed isFree = tex2D ( _StateRT , i.posBM.zw);//pm uv
				//return (_posM - posM) * isFree;
				//return _posM - posM;
				//return float2(0,0);
				return _posM * isFree + posM * (1-isFree);
			}
			ENDCG
			
		}
		
	}
}
