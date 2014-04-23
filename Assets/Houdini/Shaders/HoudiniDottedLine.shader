Shader "Houdini/DottedLine" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader 
	{

		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass 
		{
			CGPROGRAM

				#pragma exclude_renderers flash
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				float4 _Color;
				sampler2D _MainTex;
				uniform float4 _MainTex_ST; // Needed for TRANSFORM_TEX(v.texcoord, _MainTex)

				struct appdata 
				{
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f 
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert( appdata v ) 
				{
					v2f o;
					o.pos = mul( UNITY_MATRIX_MVP, v.pos );
					o.uv = TRANSFORM_TEX( v.uv, _MainTex );
					return o;
				}

				half4 frag( v2f i ) : COLOR 
				{
					half4 texcol = tex2D( _MainTex, i.uv );

					return texcol * _Color;
				}

			ENDCG
		}
	}
}
