Shader "MirrorVerse/FadingMembrane"
{
	Properties
	{
	    _MembraneColor ("Membrane color", Color) = (0, 0, 0, 1)
		_NearAlpha ("Near Alpha", float) = 1
		_FarAlpha ("Far Alpha", float) = 0
	    _NearDistance ("Near distance", float) = 0.1
		_FarDistance ("Far distance", float) = 5
		_CameraScale ("Camera scale", float) = 1.0
	}
	
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest Always

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "DepthAlpha.cginc"
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _MembraneColor;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float depthAlpha : TEXCOORD2;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depthAlpha = DepthAlpha(v.vertex); // distance from vertex to camera.
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(_MembraneColor.rgb, _MembraneColor.a * i.depthAlpha);
			}

			ENDCG
		}
	}
}
