//
////Yves Wang @ FISH, 2015, All rights reserved
//
//Shader "ParticlePhysics2D/AngleDelta" {
//	Properties {
//		_PositionRT ("_PositionRT", 2D) = "white" {}
//		_AngleRT_AB ("_AngleRT_AB",2D) = "white" {}
//		_AngleRT_M ("_AngleRT_M",2D) = "white" {}
//		_AngleRelaxPercent ( "AngleRelaxPercent" , Range(0.001,0.99)) = 0.9
//	}
//	SubShader {
//		Tags { 
//			"Queue"="Transparent" 
//			"IgnoreProjector"="True" 
//			"RenderType"="Transparent" 
//			"ForceNoShadowCasting" = "True" 
//			"PreviewType"="Plane"
//		}
//		
//		blend off
//		Zwrite off
//		fog {mode off}
//		ColorMask R
//		lighting off
//		
//		Pass {
//			
//			CGPROGRAM
//			#include "UnityCG.cginc"
//			#pragma vertex vert_fullquad
//			#pragma fragment frag
//			
//			uniform sampler2D _PositionRT;
//			uniform sampler2D _AngleRT_AB;
//			uniform sampler2D _AngleRT_M;
//			uniform float _AngleRelaxPercent;
//			const float PI2 = 6.28318530718f;
//			
//			struct appdata_fullquad
//			{
//				half4 vertex : POSITION;
//				half2 texcoord : TEXCOORD0;
//			};
//			
//			struct v2f_fullquad
//			{
//				half4 pos : SV_POSITION;
//				half2 uv : TEXCOORD0;
//			};
//			
//			float GetDeltaAngle ( float2 g1, float2 gm , float2 g2,float fixedAngle) {
//				float2 s1 = g1 - gm;
//				float2 s2 = g2 - gm;
//				float r = atan2(s2.y,s2.x) - atan2(s1.y,s1.x);//get the signed angle
//				if (r<0) r += PI2 ;//the current angle
//				return r - fixedAngle;//the delta angle
//			}
//			
//			v2f_fullquad vert_fullquad( appdata_fullquad v )
//			{
//				v2f_fullquad o;
//				o.pos = v.vertex;//because we already assgined correct screen space co-ordinates in mesh setup
//				o.uv = v.texcoord;
//				return o;
//			}
//			
//			float frag(v2f_fullquad i) : SV_Target {
//				float4 ab = tex2D ( _AngleRT_AB , i.uv );
//				float2 posA = tex2D ( _PositionRT , ab.xy );
//				float2 posB = tex2D ( _PositionRT , ab.zw );
//				float4 m = tex2D ( _AngleRT_M , i.uv);
//				float2 posM = tex2D ( _PositionRT , m.xy );
//				float fixedAngle = m.z;
//				return GetDeltaAngle(posA,posM,posB,fixedAngle) * _AngleRelaxPercent;
//				//return frac(posA.x);
//			}
//			
//			ENDCG
//		}
//		
//		
//	} //subshader
//}
