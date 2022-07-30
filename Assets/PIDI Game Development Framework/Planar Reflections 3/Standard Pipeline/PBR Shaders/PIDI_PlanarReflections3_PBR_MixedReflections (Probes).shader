/*
 * PIDI Planar Reflections 3
 * Developed  by : Jorge Pinal Negrete.
 * Copyright© 2015-2020, Jorge Pinal Negrete.  All Rights Reserved. 
 *  
*/
Shader "PIDI Shaders Collection/Planar Reflections 3/PBR/Static probes + Real-time mix" {
	Properties {
		
		[Space(12)]
		[Header(Dynamic Reflection Properties)]
		_ReflectionTint("Reflection Tint", Color) = (1,1,1,1) //The color tint to be applied to the reflection
		[PerRendererData] _ReflectionTex ("Reflection Texture", 2D) = "black" {} //The render texture containing the real-time reflection
		[PerRendererData] _ReflectionDepth("Reflection Depth", 2D) = "white"{}


	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard noshadow
		#pragma target 2.0

		sampler2D _MainTex;
		sampler2D _ReflectionTex;
		sampler2D _ReflectionDepth;
		samplerCUBE _CubemapRef;

		half _BoxMode;
		fixed4 _BoxPosition;
		fixed4 _BoxSize;

		struct Input {
			float4 screenPos;
			float3 worldRefl;
			fixed3 worldPos;
			INTERNAL_DATA
		};

		fixed4 _ReflectionTint;
		float4 _EyeOffset;
		fixed4 _Color;		
        fixed4 _ChromaKeyColor;		
        half _ChromaTolerance;
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			//We calculate the screen UV coordinates ( and ensure IN.screenPos.w is never 0 )
			float2 screenUV = IN.screenPos.xy / max( IN.screenPos.w, 0.0001 );
			
			screenUV.x = 1-screenUV.x;

			half4 c = half4(0,0,0,0);
			
			c = tex2D( _ReflectionTex, screenUV );

			o.Albedo = 0;

			o.Smoothness = 1;
			o.Metallic = 0;
			
			//o.Emission = c.rgb * (1-floor(tex2D(_ReflectionDepth,screenUV).g))*_ReflectionTint*_ReflectionTint.a;
			
			o.Alpha = 1;
		}
		
		
		ENDCG
		
		
	

		
	} 
	FallBack "Diffuse"
}
