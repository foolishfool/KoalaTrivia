/*
 * PIDI Planar Reflections 3
 * Developed  by : Jorge Pinal Negrete.
 * Copyright© 2015-2020, Jorge Pinal Negrete.  All Rights Reserved. 
 *  
*/
Shader "PIDI Shaders Collection/Planar Reflections 3/Internal/Reflections Depth Shader"
{
	Properties{
		
		_Planar3DepthPlaneOrigin("Plane Origin", Vector ) = (0,0,0,0)
		_Planar3DepthPlaneNormal("Plane Normal", Vector ) = (0,-1,0,0)
		_MainTex("Main Texture", 2D ) = "white"{}

	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100


		ZTest LEqual
		ZWrite On
		Cull Back

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag		
			#include "UnityCG.cginc"


			struct v2f{
				float2 screenUV : TEXCOORD0;
                float2 uv : TEXCOORD1;
                fixed4 color : COLOR;
                float4 pos : SV_POSITION;
			};

			float4 _Planar3DepthPlaneOrigin;
			float4 _Planar3DepthPlaneNormal;
			sampler2D _MainTex;
			half _Cutoff;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, v.vertex));
				o.uv = v.texcoord;
                o.color.rgb = distance(_Planar3DepthPlaneOrigin.xyz*_Planar3DepthPlaneNormal.xyz,mul(unity_ObjectToWorld, v.vertex).xyz*_Planar3DepthPlaneNormal.xyz)/16;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = half4(0,length(i.color.rgb),0,1);
				return col;
			}
			ENDCG
		}

		
	}
}
