

//Yves Wang @ FISH, 2015, All rights reserved

Shader "ParticlePhysics2D/Angle" {
	Properties {
		_AngleParamRT ("_AngleParamRT",2D) = "white" {}
		_AngleDeltaRT ("_AngleDeltaRT",2D) = "white" {}
		_PositionRT ("_PositionRT",2D) = "white" {}
	}
	
	CGINCLUDE
		#include "UnityCG.cginc"
		#include "QuadBlit.cginc"
		uniform sampler2D _AngleParamRT;
		uniform sampler2D _PositionRT;
		uniform sampler2D _AngleDeltaRT;
		
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
		blend off
		Zwrite off
		fog {mode off}
		ColorMask RG
		
		//0
		Pass {
			Name "AngleConstraintForOneEnd"
			CGPROGRAM
			#pragma vertex vert_quadblit
			#pragma fragment frag
			
			float2 frag(v2f_quadblit i) : SV_Target {
				float4 params = tex2D (_AngleParamRT , i.uv );
				float2 posB = tex2D (_PositionRT , i.uv);//this end
				float2 posM = tex2D (_PositionRT , params.xy);//the other end
				float delta = tex2D ( _AngleDeltaRT , params.zw);
				return RotatePoint2D ( posB , posM , -delta);
			}
			
			ENDCG
		}
		
		
	}//subshader
}//shader
