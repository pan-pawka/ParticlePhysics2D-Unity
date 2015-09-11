
//Yves Wang @ FISH, 2015, All rights reserved

Shader "FISH/ParticlePhysics2D/AngleConstraint" {
	Properties {
		_PositionRT ("", 2D) = "white" {}
		_StateRT ("",2D) = "white" {}
		_AngleRelaxPercent ( "" , Range(0.001,0.99)) = 0.9
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "ForceNoShadowCasting" = "True" }
		blend off
		Zwrite off
		fog {mode off}
		
		CGINCLUDE
		#include "UnityCG.cginc"
		sampler2D _PositionRT;
		uniform sampler2D _StateRT;
		uniform float _AngleRelaxPercent;
		const float PI2 = 6.28318530718;
		
		struct appdata_angleVert {
			float3 vertex : POSITION;//vertex.xy = A texcocord, z = fixedangle
			float4 texco : COLOR0;//xy = B texcoord, zw = M texcoord, assign in mesh setup
			float delta : TEXCOORD0;
			float4 posAB : TEXCOORD1;//xy = particle A pos,zw = particle B pos
			float2 posM : TEXCOORD2;//xy = pos M
		};
		
		struct v2f_angle {
			float2 pos : SV_POSITION;//screen space -1 to 1
			float delta : TEXCOORD0;//delta
			float2 uvP : TEXCOORD1; //the uv for the sampling point
			float4 posAB : COLOR0;//xy = particle A pos,zw = particle B pos
			float2 posM : COLOR1;//xy = pos M
		};
		
		float GetDeltaAngle ( float2 g1, float2 gm , float2 g2,float fixedAngle) {
			float2 s1 = g1 - gm;
			float2 s2 = g2 - gm;
			float r = atan2(s2.y,s2.x) - atan2(s1.y,s1.x);//get the signed angle
			if (r<0) r += PI2 ;//the current angle
			return r - fixedAngle;//the delta angle
		}
		
		//rotate point t around point c for degree a
		float2 RotatePoint2D(float2 t, float2 c, float a){
			float cosA = cos(a);
			float sinA = sin(a);
			return float2 (
				(cosA * (t.x - c.x) - sinA * (t.y - c.y) + c.x),
				(sinA * (t.x - c.x) + cosA * (t.y - c.y) + c.y)
			);
			
		}
		
		
		ENDCG
		
		//0
		//actually pass 0 and pass 1 can be combined, but for readability I seperate them
		Pass {
			Name "AngleDelta"
			ColorMask 0 //we dont need to output frag
			
			CGPROGRAM
			#pragma vertex vert_delta
			#pragma fragment frag_delta
			#pragma target 3.0
			
			v2f_angle vert_delta(appdata_angleVert IN){
			
				//get the delta
				v2f_angle OUT;
				float2 posA = tex2Dlod(_PositionRT , IN.vertex.xy);
				float2 posB = tex2Dlod(_PositionRT , IN.texco.xy);
				float2 posM = tex2Dlod(_PositionRT , IN.texco.zw);
				IN.delta = GetDeltaAngle ( posA,posB,posM,IN.vertex.z);
				IN.posAB.xy = posA;
				IN.posAB.zw = posB;
				IN.posM = posM;
				return OUT;
			}
			
			void frag_delta(v2f_angle i) : SV_Target {
				discard;
			}
			
			ENDCG
		}
		
		//1
		pass {
			Name "AngleParticleA"
			ColorMask RG
			CGPROGRAM
			#pragma vertex vert_A
			#pragma fragment frag_angle_A
			#pragma target 3.0
			
			v2f_angle vert_A (appdata_angleVert IN) {
				v2f_angle OUT;
				OUT.pos = IN.vertex.xy * 2 - 1;
				OUT.uvP = IN.vertex.xy;
				OUT.delta = IN.delta;
				OUT.posAB = IN.posAB;
				OUT.posM = IN.posM;
				return OUT;
			}
			
			float2 frag_angle_A(v2f_angle IN) : SV_Target{
				bool isFree = tex2D ( _StateRT , IN.uvP );
				float2 posA = RotatePoint2D(IN.posAB.xy , IN.posM , IN.delta * _AngleRelaxPercent );
				return posA * isFree + IN.posAB.xy * !isFree;
			}
		
			ENDCG
			
		} 
		
		//2
		pass {
			Name "AngleParticleB"
			ColorMask RG
			CGPROGRAM
			#pragma vertex vert_B
			#pragma fragment frag_angle_B
			#pragma target 3.0
			
			v2f_angle vert_B (appdata_angleVert IN) {
				v2f_angle OUT;
				OUT.pos = IN.texco.xy * 2 - 1;
				OUT.uvP = IN.texco.xy;
				OUT.delta = IN.delta;
				OUT.posAB = IN.posAB;
				OUT.posM = IN.posM;
				return OUT;
			}
			
			float2 frag_angle_B(v2f_angle IN) : SV_Target{
				bool isFree = tex2D ( _StateRT , IN.uvP );
				float2 posB = RotatePoint2D(IN.posAB.zw , IN.posM , - IN.delta * _AngleRelaxPercent );
				return posB * isFree + IN.posAB.zw * !isFree;
			}
			ENDCG
			
		}
		
		//3
		pass {
			Name "AngleParticleM"
			ColorMask RG
			CGPROGRAM
			#pragma vertex vert_M
			#pragma fragment frag_angle_M
			#pragma target 3.0
			
			v2f_angle vert_M (appdata_angleVert IN) {
				v2f_angle OUT;
				OUT.pos = IN.texco.zw * 2 - 1;
				OUT.uvP = IN.texco.zw;
				OUT.delta = IN.delta;
				OUT.posAB = IN.posAB;
				OUT.posM = IN.posM;
				return OUT;
			}
			
			float2 frag_angle_M(v2f_angle IN) : SV_Target{
				bool isFree = tex2D ( _StateRT , IN.uvP );
				float2 posM = RotatePoint2D(IN.posM , IN.posAB.xy , IN.delta * _AngleRelaxPercent );
				posM = RotatePoint2D(posM , IN.posAB.zw , - IN.delta * _AngleRelaxPercent );
				return posM * isFree + IN.posM * !isFree;
			}
			ENDCG
			
		}
		
	}
}
