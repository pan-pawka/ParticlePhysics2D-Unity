Shader "test" {
	Properties {
		
		
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "ForceNoShadowCasting" = "True" }
		//blend One One
		Pass {
			Zwrite off
			fog {mode off}
			//ColorMask RGBA
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_test
			#pragma fragment frag
			
			struct appdata_test
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};
			
			struct v2f_test
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float4 fpos : TEXCOORD1;
			};
			
			v2f_test vert_test( appdata_test v )
			{
				v2f_test o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				o.fpos = o.pos;
				return o;
			}

			float4 frag(v2f_test i) : SV_Target {
				return (i.fpos + 1)/2;
				//if (i.fpos.x == 0) return fixed4(0,0,0,1); else
				//return fixed4((i.fpos.x + 1)/2,(i.fpos.y + 1)/2,1,1);
				//return fixed4((i.fpos.y + 1)/2,1,1,1);
				discard;//use void return type and discard to skip the fragment,must use discard, or it crashes
			}
			
			ENDCG
		}
		
		
	} //subshader
}
