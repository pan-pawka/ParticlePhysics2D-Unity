Shader "ParticlePhysics2D/DebugRT" {
	Properties {
		_MainTex ("", 2D) = "white" {}
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
		LOD 200
		
		blend off
		Zwrite off
		fog {mode off}
		lighting off
		//ColorMask RG
		
		Pass {
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			
			float4 ColorNormalize( float4 t) {
				float4 b = log10(t);
				float4 f = floor(b);
				float4 base = pow(10,f+1);
				return t/base;
			}
			
			float4 frag(v2f_img i) : SV_Target {
				float4 pos = tex2D ( _MainTex , i.uv );
				//return ColorNormalize(pos);
				return frac(pos);
				//return pos;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
