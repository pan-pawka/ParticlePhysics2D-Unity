

//Yves Wang @ FISH, 2015, All rights reserved

Shader "ParticlePhysics2D/Spring" {
	Properties {
		_SpringParamRT ("_SpringParamRT",2D) = "white" {}
		_PositionRT ("_PositionRT",2D) = "white" {}
		_SpringConstant ( "_SpringConstant" , Range(0.001,0.99)) = 0.99
	}
	
	CGINCLUDE
		#include "UnityCG.cginc"
		#include "QuadBlit.cginc"
		uniform float _SpringConstant;
		uniform sampler2D _SpringParamRT;
		uniform sampler2D _PositionRT;
		
		float sqrMagnitude ( float2 v) {
			return v.x * v.x + v.y * v.y;
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
		blend off
		Zwrite off
		fog {mode off}
		ColorMask RG
		
		//0
		Pass {
			Name "SpringConstraintForOneEnd"
			CGPROGRAM
			#pragma vertex vert_quadblit
			#pragma fragment frag
//			Vector2 delta = a.Position - b.Position;
//			delta *= restLength2 /(delta.sqrMagnitude + restLength2) - 0.5f;
//			if (a.IsFree) a.Position += delta * sim.springConstant;
//			if (b.IsFree) b.Position -= delta * sim.springConstant;
			float2 frag(v2f_quadblit i) : SV_Target {
				float4 params = tex2D (_SpringParamRT , i.uv );
				float2 posA = tex2D (_PositionRT , i.uv);
				float2 posB = tex2D (_PositionRT , params.xy);
				float2 delta = posA - posB;
				float2 deltaM = delta * (params.z / ( sqrMagnitude(delta) + params.z ) - 0.5);
				float2 result = posA + deltaM * params.w * _SpringConstant;
				return result;
				
			}
			
			ENDCG
		}
		
		
	}//subshader
}//shader
