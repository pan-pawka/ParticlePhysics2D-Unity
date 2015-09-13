

//Yves Wang @ FISH, 2015, All rights reserved

Shader "FISH/ParticlePhysics2D/SpringConstraint" {
	Properties {
		_StateRT ("",2D) = "white" {}
		_SpringDeltaRT ( "" , Range(0.001,0.99)) = 0.9
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "ForceNoShadowCasting" = "True" }
		blend One One
		Zwrite off
		fog {mode off}
		
		CGINCLUDE
		#include "UnityCG.cginc"
		uniform sampler2D _StateRT;
		uniform sampler2D _SpringDeltaRT;
		
		struct appdata_springVert {
			float3 vertex : POSITION;//vertex.xy = spring rt uv
			float4 cl : COLOR;//xy = a uv in PositionRT, zw = b
		};
		
		struct v2f_spring {
			float2 pos : SV_POSITION;//screen space -1 to 1
			float4 uv : TEXCOORD0;//xy = uv in spring rt,zw = uv in PositionRT
		};
		
		float2 frag_spring(v2f_spring i) : SV_Target {
			bool isFree = tex2D ( _StateRT , i.uv.zw );
			float2 delta = tex2D ( _SpringDeltaRT , i.uv.xy) * isFree;
			return delta;
		}
		
		ENDCG
		
		//0
		Pass {
			Name "ParticleA"
			ColorMask RG 
			
			CGPROGRAM
			#pragma vertex vert_A
			#pragma fragment frag_spring
			#pragma target 3.0
			
			v2f_spring vert_A(appdata_springVert IN){
				v2f_spring OUT;
				OUT.pos = IN.cl.xy * 2 - 1;
				OUT.uv.xy = IN.vertex.xy;//uv in spring rt
				OUT.uv.zw = IN.cl.xy;//uv in positionrt
				return OUT;
			}
			ENDCG
		}
		
		//1
		Pass {
			Name "ParticleB"
			ColorMask RG 
			
			CGPROGRAM
			#pragma vertex vert_B
			#pragma fragment frag_spring
			#pragma target 3.0
			
			v2f_spring vert_A(appdata_springVert IN){
				v2f_spring OUT;
				OUT.pos = IN.cl.zw * 2 - 1;
				OUT.uv.xy = IN.vertex.xy;//uv in spring rt
				OUT.uv.zw = IN.cl.zw;//uv in positionrt
				return OUT;
			}
			
			
			
			ENDCG
		}
		
	}
}
