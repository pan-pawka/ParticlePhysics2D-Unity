Shader "Custom/multipasstest" {
	
	SubShader {
		Tags {
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"ForceNoShadowCasting" = "True" 
		}
		//blend One One
		Zwrite off
		fog {mode off}
		
		CGINCLUDE
		#include "UnityCG.cginc"
		
		uniform float4 colortest;
		
		struct appdata_mt {
			float4 vertex : POSITION;//vertex.xy = A texcocord, z = restlengt2
		};
		
		struct v2f_mt {
			float4 pos : SV_POSITION;//screen space -1 to 1
			float4 color : COLOR;
		};
		
		ENDCG
		
		//0
		Pass {
			
			//ColorMask 0 //we dont need to output frag
			
			CGPROGRAM
			#pragma vertex vert_delta
			#pragma fragment frag_delta
			#pragma target 3.0
			
			v2f_mt vert_delta(inout appdata_mt IN){
			
				UNITY_INITIALIZE_OUTPUT (appdata_mt,IN);
 
				//get the delta
				v2f_mt OUT;
				IN.vertex -= 0.5;
				OUT.pos = mul ( UNITY_MATRIX_MVP , IN.vertex );
				colortest = float4(0,1,0,0);
				OUT.color = colortest;
				return OUT;
			}
			
			float4 frag_delta(v2f_mt i) : SV_Target {
				//discard;
				return i.color;
			}
			
			ENDCG
		}
		
		//1
		Pass {
			
			CGPROGRAM
			#pragma vertex vert_delta1
			#pragma fragment frag_delta1
			#pragma target 3.0
			
			v2f_mt vert_delta1(inout appdata_mt IN){
				UNITY_INITIALIZE_OUTPUT (appdata_mt,IN);
				//get the delta
				v2f_mt OUT;
				OUT.pos = mul ( UNITY_MATRIX_MVP , IN.vertex );
				//OUT.color = colortest;
				return OUT;
			}
			
			float4 frag_delta1(v2f_mt i) : SV_Target {
				//discard;
				return float4(0,0,1,1);
			}
			
			ENDCG
		}

	} 
}
