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
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float size : PSIZE;
				};

				v2f vert( appdata v ) {
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.pos );
					o.size = 10.0;
					return o;
				}

				half4 frag( v2f i ) : COLOR {
					return half4( 0.0, 0.2, 0.2, 1 );
				}

			ENDCG
		}

		Pass {
			CGPROGRAM

				#pragma exclude_renderers flash
				#pragma vertex vert
				#pragma fragment frag

				struct appdata {
					float4 pos : POSITION;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float size : PSIZE;
				};

				v2f vert( appdata v ) {
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.pos );
					o.size = 6.0;
					return o;
				}

				half4 frag( v2f i ) : COLOR {
					return half4( 1, 1, 0, 1 );
				}

			ENDCG
		}
	}
}
