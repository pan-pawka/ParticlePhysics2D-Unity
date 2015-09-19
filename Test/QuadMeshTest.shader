Shader "ParticlePhysics2D/VerletGPUIntegrator" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
		
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
		//ColorMask RG
		
		Pass {
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_fullquad
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			
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
				//o.pos = mul ( UNITY_MATRIX_MVP , v.vertex );
				o.uv = v.texcoord;
				return o;
			}
			
			float4 frag(v2f_fullquad i) : SV_Target {
				float4 co = tex2D ( _MainTex , i.uv) * float4 ( 1,1,1,1);
				return co;
			}
			
			ENDCG
		}
		
		
	} //subshader
}