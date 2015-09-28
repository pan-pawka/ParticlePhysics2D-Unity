

//Yves Wang @ FISH, 2015, All rights reserved

Shader "ParticlePhysics2D/GPULineRenderer" {
	Properties {
		_Color ("Line Color", Color) = (1,1,1,1)
		_PositionRT ("_PositionRT",2D) = "white" {}
		_LineWidth ( "_LineWidth" , Range(0.001,0.99)) = 0.99
	}
	
	CGINCLUDE
		#include "UnityCG.cginc"
		uniform fixed4 _Color;
		uniform sampler2D _PositionRT;
		uniform sampler1D _LineFilterRT;//pre caculated line intensity filter, 0 - 1 space
		uniform float _LineWidth;
		
		struct appdata
		{
			half4 vertex : POSITION;//xy = position z = level length, (level int or real branch length are diff effects)
			//(positive means width index=1 - center point,negative means width index = 0 - edge point,level degin from 1 instead of 0)
			half2 texcoord : TEXCOORD0;//xy = mid point uv,
			half4 cl:COLOR;//rg = pointA uv, ba = pointB uv
			
		};
		
		struct v2f
		{
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;//x = 0-1 width index, y = level
			
		};
		
		//sin (a/2) = sqrt((1-cos a)/2); half sin law
		float2 AngledPoint ( float2 posA,float2 posM, float2 posB) {
			float2 nA = normalize (posA - posM);
			float2 nB = normalize (posB - posM);
			float2 nM = normalize (nA + nB);
			float cosa = dot(nA,nB);
			float sina2 = sqrt((1-cosa)/2);
			float l = _LineWidth / sina2;
			return l * nM;
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
		Blend SrcAlpha OneMinusSrcAlpha
		Zwrite off
		fog {mode off}
		
		//0
		Pass {
			Name "GPULineRenderer"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			v2f vert(appdata IN) : SV_POSITION {
				v2f o;
				//get width index and level
				half level = IN.vertex.z;
				o.uv.x = (level >= 0);
				o.uv.y = abs(level);
				
				float2 posM = tex2Dlod ( _PositionRT , IN.texcoord );
				IN.vertex.z = 0;
				
				//if its center point
				if (level >=0 ) {
					IN.vertex.xy = posM;
				} else {
					float2 posA = tex2Dlod ( _PositionRT , IN.cl.xy );
					float2 posB = tex2Dlod ( _PositionRT , IN.cl.zw );
					IN.vertex.xy = AngledPoint ( posA, posM, posB);
				}
				o.pos = mul ( UNITY_MATRIX_MVP , IN.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target {
				fixed intensity = tex1D ( _LineFilterRT , i.uv.x);
				return fixed4(_Color.xyz,intensity);//level is not used for now
			}
			
			ENDCG
		}
		
		
	}//subshader
}//shader
