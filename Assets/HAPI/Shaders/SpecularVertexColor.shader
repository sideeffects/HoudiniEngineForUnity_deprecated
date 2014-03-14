Shader "HAPI/SpecularVertexColor" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Spec Color", Color) = (1,1,1,1)
		_Emission ("Emmisive Color", Color) = (0,0,0,0)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.7
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SpecularMap("Specular Map", 2D) = "black" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_BumpMap ("Bump Map", 2D) = "bump" {}
		_DisplacementMap ("Displacement Map", 2D) = "black" {}
	}

	SubShader {
		Pass {
			Material {
				Shininess [_Shininess]
				Specular [_SpecColor]
				Emission [_Emission]
			}
			ColorMaterial AmbientAndDiffuse
			Lighting On
			SeparateSpecular On
			SetTexture [_MainTex] {
				Combine texture * primary, texture * primary
			}
			SetTexture [_MainTex] {
				constantColor [_Color]
				Combine previous * constant DOUBLE, previous * constant
			}
		}
	}

	Fallback "Specular", 1
}
