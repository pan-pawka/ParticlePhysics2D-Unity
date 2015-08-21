//Copyright(c),2015 - Yves Wang @ FISH - All rights reserved

Shader "FISH/GaussBlur05" {

	Properties {
		
		_MainTex ("Particle lookup", 2D) = "white" {}
		
	}

	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"ForceNoShadowCasting" = "True"
			
		}
		//LOD 200
		Pass {
			//Cull Off 
			ZWrite off
			Fog {Mode off}

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			uniform half _ScreenDeltaU;
			uniform half _ScreenDeltaV;
			//float3x3 gaussMatrix = float3x3(12/255,31/255,12/255,31/255,83/255,31/255,12/255,31/255,12/255);
			//const float3x3 gaussMatrix = float3x3(0.04705882352,0.12156862745,0.04705882352,0.12156862745,0.32549019607,0.12156862745,0.04705882352,0.12156862745,0.04705882352);
			
			fixed4 frag(v2f_img i) : SV_Target {
            	
            	fixed4 o = fixed4(0,0,0,0);
            	
            	fixed4 c1 = tex2D(_MainTex,i.uv + half2(-_ScreenDeltaU,_ScreenDeltaV));//top left
            	fixed4 c2 = tex2D(_MainTex,i.uv + half2(0,_ScreenDeltaV));//top middle
            	fixed4 c3 = tex2D(_MainTex,i.uv + half2(_ScreenDeltaU,_ScreenDeltaV));//top right
            	fixed4 c4 = tex2D(_MainTex,i.uv);//middle
            	fixed4 c5 = tex2D(_MainTex,i.uv + half2(-_ScreenDeltaU,0));//middle left 
            	fixed4 c6 = tex2D(_MainTex,i.uv + half2(_ScreenDeltaU,0));//middle right
            	fixed4 c7 = tex2D(_MainTex,i.uv + half2(-_ScreenDeltaU,-_ScreenDeltaV));//bottom left
            	fixed4 c8 = tex2D(_MainTex,i.uv + half2(0,-_ScreenDeltaV));//bottom middle
            	fixed4 c9 = tex2D(_MainTex,i.uv + half2(_ScreenDeltaU,-_ScreenDeltaV));//bottom right
            	
				o += 0.04705882352 * (c1+c3+c7+c9);
            	o += 0.12156862745 * (c2+c5+c6+c8);
            	o += 0.32549019607 * c4;
            	
            	return o;
            }
            
            
			ENDCG
		}
	}
}