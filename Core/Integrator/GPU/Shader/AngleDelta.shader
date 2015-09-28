
//Yves Wang @ FISH, 2015, All rights reserved

Shader "ParticlePhysics2D/AngleDelta" {
	Properties {
		_PositionRT ("_PositionRT", 2D) = "white" {}
		_AngleConstant ( "_AngleConstant" , Range(0.001,0.99)) = 0.99
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
		ColorMask R
		lighting off
		//cull off
		
		Pass {
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _PositionRT;
			uniform float _AngleConstant;
			const float PI2 = 6.28318530718f;
			
			struct appdata
			{
				half4 vertex : POSITION;//xy = delta rt uv, z = fixed angle
				half2 texcoord : TEXCOORD0;//xy = a uv
				half4 cl:COLOR;//rg = b uv, ba = m uv
				
			};
			
			struct v2f
			{
				half4 pos : SV_POSITION;
				half4 uv : TEXCOORD0;//xy = a uv,z = fixed angle
				half4 cl : COLOR;//rg = b uv, ba = m uv
			};
			
			float GetDeltaAngle ( float2 g1, float2 gm , float2 g2,float fixedAngle) {
				float2 s1 = g1 - gm;
				float2 s2 = g2 - gm;
				float r = atan2(s2.y,s2.x) - atan2(s1.y,s1.x);//get the signed angle
				if (r<0) r += PI2 ;//the current angle
				return r - fixedAngle;//the delta angle
			}
			
			v2f vert( appdata v )
			{
				v2f o;
				o.uv = half4(v.texcoord.x,v.texcoord.y,v.vertex.z,0);
				v.vertex.z = 0;//this is where I was caught by a bug....have to explicitly set it to 0 before mul with mvp
				o.pos = mul ( UNITY_MATRIX_MVP , v.vertex);
				o.pos.xy = v.vertex.xy * 2 -1;
				o.cl = v.cl;
				
				return o;
			}
			
			float frag(v2f i) : SV_Target {
				float2 posA = tex2D ( _PositionRT , i.uv.xy);
				float2 posB = tex2D ( _PositionRT , i.cl.rg);
				float2 posM = tex2D ( _PositionRT , i.cl.ba);
				float fixedAngle = i.uv.z;
				return GetDeltaAngle(posA,posM,posB,fixedAngle) * _AngleConstant;
			}
			
			ENDCG
		}
		
		
	} //subshader
}
