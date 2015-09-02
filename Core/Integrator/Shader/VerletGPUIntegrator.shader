
//Yves Wang @ FISH, 2015, All rights reserved

Shader "FISH/ParticlePhysics2D/VerletGPUIntegrator" {
	Properties {
		_MainTex ("", 2D) = "white" {}
		_PositionCache ("",2D) = "white" {}
		//_StateTex ("",2D) = "white" {} //because if particle is not changed during constraint or collision ,then the curPos and prePos will be the same always
		_Damping ( "" , Range(0.001,0.99)) = 0.9
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
			#pragma vertex vert_img
			#pragma fragment frag
			
			sampler2D _MainTex;
			uniform sampler2D _PositionCache;
			uniform float _Damping;
			
			float2 frag(v2f_img i) : SV_Target {
				float2 curPos = tex2D ( _MainTex , i.uv );
				float2 prePos = tex2D ( _PositionCache , i.uv );
				return curPos + (curPos - prePos) * _Damping;
			}
			
			ENDCG
		}
		
		
	} //subshader
}
