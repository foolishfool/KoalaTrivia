/*
 * PIDI Planar Reflections 3
 * Developed  by : Jorge Pinal Negrete.
 * Copyright© 2015-2020, Jorge Pinal Negrete.  All Rights Reserved. 
 *  
*/
Shader "PIDI Shaders Collection/Planar Reflections 3/PBR/Static + Real-time mix" {
	Properties {
		
		[Space(12)]
		[Header(Dynamic Reflection Properties)]
		_ReflectionTint("Reflection Tint", Color) = (1,1,1,1) //The color tint to be applied to the reflection
		[PerRendererData] _ReflectionTex ("Reflection Texture", 2D) = "black" {} //The render texture containing the real-time reflection
		[PerRendererData] _ReflectionDepth("Reflection Depth", 2D) = "white"{}

		[Space(12)]
		[Header(Static Reflection Properties)]
        _CubemapRef("Cubemap Reflection", CUBE ) = ""{} //Pre-baked cubemap to mix
		[Toggle]_BoxMode("Use Box Projection",Float) = 0
		_BoxPosition ("Env Box Start", Vector) = (0, 0, 0)
		_BoxSize ("Env Box Size", Vector) = (10, 10, 10)

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert noshadow
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
		
		void surf (Input IN, inout SurfaceOutput o) {
		
			//We calculate the screen UV coordinates ( and ensure IN.screenPos.w is never 0 )
			float2 screenUV = IN.screenPos.xy / max( IN.screenPos.w, 0.0001 );
			
			screenUV.x = 1-screenUV.x;

			half4 c = half4(0,0,0,0);
			
			c = tex2D( _ReflectionTex, screenUV );

			//BPCM
			fixed3 nReflDirection = normalize(WorldReflectionVector (IN, o.Normal));
   
			float3 boxStart = _BoxPosition - _BoxSize / 2.0;
			float3 firstPlaneIntersect = (boxStart + _BoxSize - IN.worldPos) / nReflDirection;
			float3 secondPlaneIntersect = (boxStart - IN.worldPos) / nReflDirection;
			float3 furthestPlane = (nReflDirection > 0.0) ? firstPlaneIntersect : secondPlaneIntersect;
			float3 intersectDistance = min(min(furthestPlane.x, furthestPlane.y), furthestPlane.z);
			float3 intersectPosition = IN.worldPos + nReflDirection * intersectDistance;
			//END BPCM

			
			o.Emission = lerp( texCUBE (_CubemapRef, lerp(WorldReflectionVector (IN, o.Normal),intersectPosition - _BoxPosition,_BoxMode)).rgb, c.rgb, 1-floor(tex2D(_ReflectionDepth,screenUV).g))*_ReflectionTint*_ReflectionTint.a;
			
			o.Alpha = 1;
		}
		
		
		ENDCG
		
		
	

		
	} 
	FallBack "Diffuse"
}
