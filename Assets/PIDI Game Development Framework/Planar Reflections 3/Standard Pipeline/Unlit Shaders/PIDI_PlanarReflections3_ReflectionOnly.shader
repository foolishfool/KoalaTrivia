﻿/*
 * PIDI Planar Reflections 3
 * Developed  by : Jorge Pinal Negrete.
 * Copyright(c) 2015-2020, Jorge Pinal Negrete.  All Rights Reserved.
 *
*/


Shader "PIDI Shaders Collection/Planar Reflections 3/Unlit/Reflection Only"
{
	Properties
	{
		[PerRendererData]_ReflectionTex ("Reflection Texture", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 screenPos : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _ReflectionTex;
			float4 _ReflectionTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeGrabScreenPos(o.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				i.screenPos.xy /= i.screenPos.w;
				i.screenPos.x = 1-i.screenPos.x;

				// sample the texture
				fixed4 col = tex2D(_ReflectionTex, i.screenPos.xy);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
