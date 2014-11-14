Shader "Houdini/VolumeSurface" {

	Properties {
		_PointSize ("PointSize", Float) = 10.0
		_Color ("Color", Color) = (1,1,1,1)
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

				float _PointSize;
				float4 _Color;

				struct a2v
				{
					float4 vertex : POSITION;
					float3 normal: NORMAL;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 color : COLOR;
					float size : PSIZE;
				};

				v2f vert (a2v v)
				{
					v2f o;

					o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
					o.color = float4( ShadeVertexLights( v.vertex, v.normal ) * 2.0, 1.0 ) * _Color;

					float3 worldSpaceObjectPos = mul( v.vertex, _Object2World ).xyz;
					float dist = distance( worldSpaceObjectPos.xyz, _WorldSpaceCameraPos.xyz );

					o.size = _PointSize * ( 1 / dist );
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

