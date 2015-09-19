
//Yves Wang @ FISH, 2015, All rights reserved

Shader "ParticlePhysics2D/SpringDelta" {
	Properties {
		_PositionRT ("_PositionRT", 2D) = "white" {}
		_SpringRT ("_SpringRT",2D) = "white" {}
		_RestLength2RT ("_RestLength2RT",2D) = "white" {}
		_SpringConstant ( "SpringConstant" , Range(0.001,0.99)) = 0.9
	}
	SubShader {
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"ForceNoShadowCasting" = "True" 
			"PreviewType"="Plane"
			"DisableBatching" = "True"
		}
		lighting off
		//cull off
		blend off
		Zwrite off
		fog {mode off}
		ColorMask RG
		
		Pass {
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_fullquad
			#pragma fragment frag
			
			uniform sampler2D _PositionRT;
			uniform sampler2D _SpringRT;
			uniform sampler2D _RestLength2RT;
			uniform float _SpringConstant;
			
			struct appdata_fullquad
			{
				half4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};
			
			struct v2f_fullquad
			{
				half4 pos : SV_POSITION;
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
				float4 pos = tex2D ( _SpringRT , i.uv );
				float2 posA = tex2D ( _PositionRT , pos.xy );
				float2 posB = tex2D ( _PositionRT , pos.zw );
				float restlength2 = tex2D ( _RestLength2RT , i.uv);
				float2 delta = posA - posB;
				////delta *= (restLength2 / (delta.x * delta.x + delta.y * delta.y + restlength2) -0.5) * constant;
				delta *= (restlength2 / (delta.x * delta.x + delta.y * delta.y + restlength2) -0.5) * _SpringConstant;
				return delta;
				//return fixed2(1,1);
				//return delta.x * delta.x + delta.y * delta.y - restlength2;
				//return posA;
			}
			
			ENDCG
		}
		
		
	} //subshader
}
