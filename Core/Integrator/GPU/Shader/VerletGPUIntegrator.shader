
//Yves Wang @ FISH, 2015, All rights reserved

Shader "ParticlePhysics2D/VerletGPUIntegrator" {
	Properties {
		_PositionRT ("PositionRT", 2D) = "white" {}
		_PositionCache ("PositionCache",2D) = "white" {}
		_Damping ( "Damping" , Range(0.001,0.99)) = 0.9
	}
	SubShader {
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"ForceNoShadowCasting" = "True" 
			"PreviewType"="Plane"
		}
		blend off
		Zwrite off
		fog {mode off}
		ColorMask RG
		
		Pass {
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "QuadBlit.cginc"
			#pragma vertex vert_quadblit
			#pragma fragment frag
			
			uniform sampler2D _PositionRT;
			uniform sampler2D _PositionCache;
			uniform float _Damping;
			
			float2 frag(v2f_quadblit i) : SV_Target {
				float2 curPos = tex2D ( _PositionRT , i.uv );
				float2 prePos = tex2D ( _PositionCache , i.uv );
				float2 result = curPos + (curPos - prePos) * _Damping;
				return result;
			}
			
			ENDCG
		}
		
		
	} //subshader
}
