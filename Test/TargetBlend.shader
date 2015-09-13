Shader "Custom/TargetBlend" {
	Properties {
		_MainTex ("", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Blend one one
		
		pass {
		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag
			
			#pragma target 3.0
	
			sampler2D _MainTex;
	
			fixed4 frag ( v2f_img IN ) : SV_Target {
				return fixed4 ( 0,0,1,1);
			}
			ENDCG
		
		
		}
		
	} 
	FallBack "Diffuse"
}
