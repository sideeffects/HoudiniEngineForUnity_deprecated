Shader "HAPI/Line" {
	SubShader {

		ZTest Less

		Pass {
			CGPROGRAM

				#pragma exclude_renderers flash
				#pragma vertex vert
				#pragma fragment frag

				struct appdata {
					float4 pos : POSITION;
					float4 colour : COLOR;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float4 colour : COLOR;
				};

				v2f vert( appdata v ) {
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.pos );
					o.colour = v.colour;
					return o;
				}

				half4 frag( v2f i ) : COLOR {
					return half4( i.colour );
				}

			ENDCG
		}
	}
}
