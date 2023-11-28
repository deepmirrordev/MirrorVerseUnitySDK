Shader "MirrorVerse/ScanLine"
{
	Properties
	{
		_MainTex("Screen", 2D) = "black" {}
		[HDR]_Colour("Colour", Color) = (1,1,1,1)
		_Origin("Origin", Vector) = (0,0,0,0)
		_Power("Power", Float) = 10
		_Tiling("Tiling", Float) = 1
		_Speed("Speed", Float) = 1
		_MaskRadius("Mask Radius", Float) = 5
		_MaskHardness("Mask Hardness", Range(0 , 1)) = 1
		_MaskPower("Mask Power", Float) = 1
		_MultiplyBlend("Multiply Blend", Range(0 , 1)) = 0
		[HideInInspector] _texcoord("", 2D) = "white" {}
		_StartTime("StartTime", Float) = -1
		_Depth("ScanLineDepth", Float) = 5

	}

	SubShader
	{
		LOD 0
		ZTest Always
		Cull Off
		ZWrite Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert_img_custom 
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "ScanLine.cginc"
			ENDCG
		}
	}
}
