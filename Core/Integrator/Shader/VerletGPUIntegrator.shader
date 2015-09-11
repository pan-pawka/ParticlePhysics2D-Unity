
//Yves Wang @ FISH, 2015, All rights reserved

Shader "FISH/ParticlePhysics2D/VerletGPUIntegrator" {
	Properties {
		_PositionRT ("", 2D) = "white" {}
		_PositionCache ("",2D) = "white" {}
		//_StateRT ("",2D) = "white" {} //because if particle is not changed during constraint or collision ,then the curPos and prePos will be the same always
		_Damping ( "Damping" , Range(0.001,0.99)) = 0.9
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "ForceNoShadowCasting" = "True" }
		blend One One
		Pass {
			Zwrite off
			fog {mode off}
			ColorMask RG
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_fullquad
			#pragma fragment frag
			
			uniform sampler2D _PositionRT;
			uniform sampler2D _PositionCache;
			uniform float _Damping;
			
			struct appdata_fullquad
			{
				half2 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};
			
			struct v2f_fullquad
			{
				half2 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};
			
			v2f_fullquad vert_fullquad( appdata_fullquad v )
			{
				v2f_fullquad o;
				o.pos = v.vertex;//because we already assgined correct screen space co-ordinates in mesh setup
				o.uv = v.texcoord;
				return o;
			}
			
			float2 frag(v2f_fullquad i) : SV_Target {
				float2 curPos = tex2D ( _PositionRT , i.uv );
				float2 prePos = tex2D ( _PositionCache , i.uv );
				return curPos + (curPos - prePos) * _Damping;
			}
			
			ENDCG
		}
		
		
	} //subshader
}
