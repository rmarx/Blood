Shader "Custom/MultiplyTexture" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {	
		LOD 100
		Tags { "Queue" = "Transparent+500" }
		Pass {						
			ZWrite off
			Lighting off
			Blend DstColor Zero
			SetTexture[_MainTex]{combine texture}
		}		
	} 
	FallBack "Diffuse"
}
