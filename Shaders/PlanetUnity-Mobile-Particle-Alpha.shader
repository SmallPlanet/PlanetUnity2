// Planet Unity:
// - Position, rotation, and size computed in vertex shader

Shader "PlanetUnity/Mobile/Particles/Alpha Blended" {
Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)

		_ScaleInfo ("Scale Info", Vector) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		
		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Back
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float4 normal    : NORMAL;
				float4 tangent    : TANGENT;
				float2 texcoord0 : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			float4 _ScaleInfo;

			v2f vert(appdata_t IN)
			{
				v2f OUT;

				// NORMAL xy contains x and y offset from IN.vertex
				// texcoord1 x contains size of the offset
				// texcoord1 y contains rotation in radians
				float c = cos (IN.texcoord1.y * 0.01745329);
				float s = sin (IN.texcoord1.y * 0.01745329);
				OUT.vertex = mul(UNITY_MATRIX_MVP, (IN.vertex * float4(_ScaleInfo.xy, 1, 1) + float4(IN.normal.x * IN.texcoord1.x * c - IN.normal.y * IN.texcoord1.x * s, IN.normal.x * IN.texcoord1.x * s + IN.normal.y * IN.texcoord1.x * c, 0, 0) * (_ScaleInfo.x + _ScaleInfo.y) * 0.5));

				OUT.texcoord = IN.texcoord0;
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				return tex2D(_MainTex, IN.texcoord) * IN.color;
			}
		ENDCG
		}
	}
}
