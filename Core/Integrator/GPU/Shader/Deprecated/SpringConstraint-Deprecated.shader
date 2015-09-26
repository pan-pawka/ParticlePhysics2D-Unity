//
//
////Yves Wang @ FISH, 2015, All rights reserved
//
//Shader "ParticlePhysics2D/SpringConstraint" {
//	Properties {
//		_StateRT ("_StateRT",2D) = "white" {}
//		_SpringDeltaRT ("_SpringDeltaRT",2D) = "white" {}
//	}
//	
//	CGINCLUDE
//		#include "UnityCG.cginc"
//		uniform sampler2D _StateRT;
//		uniform sampler2D _SpringDeltaRT;
//		
//		struct appdata_springVert {
//			float4 vertex : POSITION;//vertex.xy = spring rt uv
//			float4 cl : COLOR;//xy = a uv in PositionRT, zw = b
//		};
//		
//		struct v2f_spring {
//			float4 pos : SV_POSITION;//screen space -1 to 1
//			float4 uv : TEXCOORD0;//xy = uv in spring rt,zw = uv in PositionRT
//		};
//			
//	ENDCG
//		
//	SubShader {
//		Tags { 
//			"Queue"="Transparent"
//			"IgnoreProjector"="True" 
//			"RenderType"="Transparent" 
//			"ForceNoShadowCasting" = "True" 
//			"PreviewType"="Plane"
//		}
//		lighting off
//		blend one one
//		Zwrite off
//		fog {mode off}
//		ColorMask RG 
//		
//		//0
//		Pass {
//			Name "ParticleA"
//			CGPROGRAM
//			#pragma vertex vert_A
//			#pragma fragment frag_A
//			#pragma target 3.0
//			
//			v2f_spring vert_A(appdata_springVert IN){
//				v2f_spring OUT;
//				OUT.pos = mul ( UNITY_MATRIX_MVP , IN.vertex);//need to explicitly set zw by mul with MVP
//				OUT.pos.xy = IN.cl.xy * 2 - 1;
//				OUT.uv.xy = IN.vertex.xy;//uv in spring rt
//				OUT.uv.zw = IN.cl.xy;//uv in positionrt
//				return OUT;
//			}
//			
//			float2 frag_A(v2f_spring i) : SV_Target {
//				fixed isFree = tex2D ( _StateRT , i.uv.zw );
//				float2 delta = tex2D ( _SpringDeltaRT , i.uv.xy) ;
//				return delta * isFree;
//				//return fixed4(1,1,0,1);
//				//return float4(1,1,0,0);
//				//return float2(1,0);
//			}
//			
//			ENDCG
//		}
//		
//		//1
//		Pass {
//			Name "ParticleB"
//			
//			CGPROGRAM
//			#pragma vertex vert_B
//			#pragma fragment frag_B
//			#pragma target 3.0
//			
//			v2f_spring vert_B(appdata_springVert IN){
//				v2f_spring OUT;
//				OUT.pos = mul ( UNITY_MATRIX_MVP , IN.vertex);
//				OUT.pos.xy = IN.cl.zw * 2 - 1;
//				OUT.uv.xy = IN.vertex.xy;//uv in spring rt
//				OUT.uv.zw = IN.cl.zw;//uv in positionrt
//				return OUT;
//			}
//		
//			float2 frag_B(v2f_spring i) : SV_Target {
//				fixed isFree = tex2D ( _StateRT , i.uv.zw );
//				float2 delta = tex2D ( _SpringDeltaRT , i.uv.xy) ;
//				return -delta * isFree;
//				//return fixed4(0,1,0,1);
//			}
//			
//			ENDCG
//		}
//		
//	}
//}
