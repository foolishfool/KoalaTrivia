/*
 * PIDI Planar Reflections 3
 * Developed  by : Jorge Pinal Negrete.
 * Copyright© 2015-2020, Jorge Pinal Negrete.  All Rights Reserved.
 *
*/
Shader "PIDI Shaders Collection/Planar Reflections 3/PBR/Metallic (Broken + Alpha Blended)" {

	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}


		[Header(Glossiness)]
		[Space(10)]
		[Enum(Values,0,Texture,1)] _RghSource("Gloss Source", Float) = 0
		_Glossiness("Gloss", Range(0.0, 1.0)) = 0.5

		[Header(Metallic)]
		[Space(10)]
		[Enum(Values,0,Texture,1)] _MetSource("Metallic Source", Float) = 0
		_Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		[NoScaleOffset]_MetallicGlossMap("Metallic (R) Gloss(A)", 2D) = "white" {}

		[Header(Normals and Parallax Mapping)]
		[Space(10)]
		_BumpScale("Scale", Float) = 1.0
		[NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}

		_Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
		[NoScaleOffset]_ParallaxMap("Height Map", 2D) = "gray" {}

		[Header(Occlusion.Set to 0 when using Occlusion Maps)]
		[Space(10)]
		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		[NoScaleOffset]_OcclusionMap("Occlusion", 2D) = "white" {}

		[Space(12)]
		[Header(Material Emission)]
		[Enum(Additive,0,Masked,1)]_EmissionMode("Emission/Reflection Blend Mode", Float) = 0 //Blend mode for the emission and reflection channels
		_EmissionColor("Emission Color (RGB) Intensity (16*Alpha)", Color) = (1,1,1,0.5)
		[NoScaleOffset]_EmissionMap("Emission Map (RGB) Mask (A)", 2D) = "black"{}//Emissive map

		[Space(10)]
		[Header(Detail Textures Setup.Tiling Comes from Albedo)]
		[Space(10)]

		_DetailMask("Detail Mask", 2D) = "white" {}

		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
		_DetailNormalMap("Normal Map", 2D) = "bump" {}

		[Enum(UV1_FromAlbedo,0,UV2_FromNormalmap,1)] _UVSec("UV Set for secondary textures", Float) = 0

		[Space(12)]
		[Header(Broken Reflections)]
		_BrokenPattern("Broken Pattern Hor(R) Ver(G) Dir(B)",2D) = "gray"{}
		_BrokenOffsetX("Broken Offset X", Range(-1,1)) = 0
		_BrokenOffsetY("Broken Offset Y", Range(-1,1)) = 0

		[Space(12)]
		[Header(Reflection Properties)]
		_ReflectionTint("Reflection Tint", Color) = (1,1,1,1) //The color tint to be applied to the reflection
		_RefDistortion("Bump Reflection Distortion", Range(0, 0.1)) = 0.25 //The distortion applied to the reflection
		[Toggle]_ReflectionDepthInfluence("Depth Based Blur", Range(0, 1)) = 0 //The blur factor applied to the reflection
		_DepthBlurPower("Depth Pass Falloff", Range(0,1)) = 0.15
		_NormalDist("Surface Distortion", Range(0,1)) = 0 //Surface derived distortion
		[PerRendererData] _ReflectionTex("Reflection Texture", 2D) = "white" {} //The render texture containing the real-time reflection
		[PerRendererData] _BlurReflectionTex("Blurred Reflection Texture", 2D) = "white" {} //The render texture containing the real-time reflection
		[PerRendererData] _ReflectionDepth("Reflection Depth", 2D) = "white"{}//Reflection depth
		[PerRendererData] _HasBlurMap("Has Blur Map", Float) = 0


	}


		SubShader{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows alpha:blend noshadow nodynlightmap 
			#include "UnityStandardUtils.cginc"

			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetallicGlossMap;
			sampler2D _OcclusionMap;
			sampler2D _EmissionMap;
			sampler2D _ReflectionTex;
			sampler2D _BlurReflectionTex;
			sampler2D _ReflectionDepth;
			sampler2D _ParallaxMap;
			sampler2D _DetailAlbedoMap;
			sampler2D _DetailNormalMap;
			sampler2D _DetailMask;

			struct Input {
				float2 uv_MainTex;
				float2 uv_BrokenPattern;
				float2 uv_DetailAlbedoMap;
				float2 uv2_DetailNormalMap;
				float2 uv_DetailAlbedoMask;
				float3 viewDir;
				float4 screenPos;
			};


			half _Glossiness;
			half _Metallic;
			half _BumpScale;
			half _DetailNormalMapScale;
			half _OcclusionStrength;
			float _UVSec;
			float _RghSource;
			float _MetSource;
			fixed4 _Color;
			fixed4 _EmissionColor;
			fixed4 _ReflectionTint;
			float4 _EyeOffset;
			half _NormalDist;
			half _BlurSize;
			half _RefDistortion;
			half _Parallax;
			half _EmissionMode;
			half _DepthBlurPower;
			half _ReflectionDepthInfluence;
			half _BlurMode;

			sampler2D _BrokenPattern;

			half _BrokenOffsetX;
			half _BrokenOffsetY;



			void surf(Input IN, inout SurfaceOutputStandard o) {

				float2 offsetHeight = ParallaxOffset(tex2D(_ParallaxMap, IN.uv_MainTex).r, _Parallax, IN.viewDir);

				o.Normal = float3(0,0,1);

				half dist = 2 * sign(dot(o.Normal, IN.viewDir) - 0.5) * (dot(o.Normal,IN.viewDir) - 0.5) * _NormalDist; //Normal based distortion factor


				// Albedo comes from a texture tinted by color
				fixed4 col = tex2D(_MainTex, IN.uv_MainTex + offsetHeight) * _Color;


				float2 dUV = lerp(IN.uv_DetailAlbedoMap, IN.uv2_DetailNormalMap, _UVSec) + offsetHeight;

				half dMask = tex2D(_DetailMask, IN.uv_DetailAlbedoMask).a;
				half3 n1 = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex + offsetHeight), _BumpScale);
				half3 n2 = UnpackScaleNormal(tex2D(_DetailNormalMap, dUV), _DetailNormalMapScale * dMask);

				o.Albedo = col.rgb * lerp((tex2D(_DetailAlbedoMap, dUV).rgb * unity_ColorSpaceDouble.r), 1, 1 - dMask);
				o.Normal = normalize(half3(n1.x + n2.x,n1.y + n2.y,n1.z));

				offsetHeight = o.Normal * _RefDistortion; //We get the reflection distortion by multiplying the _RefDistortion factor by the normal.

				//Calculate the screen UV coordinates
				float2 screenUV = IN.screenPos.xy / max(IN.screenPos.w,0.001);
				screenUV.x = 1 - screenUV.x;


				screenUV += dist;


				half4 broken = tex2D(_BrokenPattern, IN.uv_BrokenPattern);

				screenUV.x += broken.r * _BrokenOffsetX * sign(broken.b - 0.5);
				screenUV.y += broken.g * _BrokenOffsetY * sign(broken.b - 0.5);

				fixed4 met = tex2D(_MetallicGlossMap, IN.uv_MainTex + offsetHeight);
				// Metallic and smoothness come from slider variables
				o.Metallic = lerp(_Metallic, met.rgb, _MetSource);
				o.Smoothness = lerp(_Glossiness, met.a, _RghSource);

				float rDepth = pow(tex2D(_ReflectionDepth, screenUV).g, 32);

				rDepth = saturate(rDepth + (1 - _DepthBlurPower) * (1 - floor(rDepth + 0.01)));

				half blur = saturate(saturate(1.1 - o.Smoothness) - lerp(0, 1 - rDepth, _ReflectionDepthInfluence));

				half4 ref1 = tex2D(_ReflectionTex, screenUV + offsetHeight);
				half4 ref2 = tex2D(_BlurReflectionTex, screenUV + offsetHeight);
				half4 c = lerp(ref1, ref2, blur);


				o.Occlusion = lerp(1,tex2D(_OcclusionMap, IN.uv_MainTex + offsetHeight).r, _OcclusionStrength);

				half4 e = tex2D(_EmissionMap,IN.uv_MainTex + offsetHeight);
				e.rgb *= _EmissionColor.rgb * (_EmissionColor.a * 16);
				half fresnelValue = saturate(dot(o.Normal, IN.viewDir)); //We calculate a very simple fresnel - like value based on the view to surface angle.
				//And use it for the reflection, since we want it to be stronger in sharp view angles and get affected by the diffuse color of the surface when viewed directly.
				o.Emission = e.rgb + lerp(1,1 - e.a,_EmissionMode) * c.rgb * _ReflectionTint * max(0.05,o.Smoothness) * lerp(_ReflectionTint.a * 0.5, 1, 1 - fresnelValue) * lerp(max(o.Albedo, half3(0.1, 0.1, 0.1)), half3(1, 1, 1), 1 - fresnelValue);
				o.Alpha = col.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}