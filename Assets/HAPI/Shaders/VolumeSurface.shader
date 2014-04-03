Shader "HAPI/VolumeSurface" {

	Properties {
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 80

		Pass {
			Lighting On

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct a2v
				{
					float4 vertex : POSITION;
					float4 color: COLOR;
				};

				struct v2f
				{
					float4 pos : POSITION;
					float4 color : COLOR;
				};

				v2f vert (a2v v)
				{
					v2f o;

					float3 normal = float3(
						v.color.r * 2.0 - 1.0,
						v.color.g * 2.0 - 1.0,
						v.color.b * 2.0 - 1.0 );

					o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
					o.color = float4( ShadeVertexLights( v.vertex, normal ) * 2.0, 1.0 );
					return o;
				}

				float4 frag(v2f i) : COLOR
				{
					return i.color;
				}
			ENDCG
		}
	}
	FallBack "VertexLit"
}

