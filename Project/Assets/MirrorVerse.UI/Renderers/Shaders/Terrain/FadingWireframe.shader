Shader "MirrorVerse/FadingWirefame"
{
	Properties
	{
	    _LineColor ("Line color", Color) = (0, 0, 0, 1)
	    _LineSize ("Line size", float) = 0.3
	    _NearDistance ("Near distance", float) = 0.1
		_FarDistance ("Far distance", float) = 5
		_NearAlpha ("Near Alpha", float) = 1
		_FarAlpha ("Far Alpha", float) = 0
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
			#include "WireLineAlpha.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			fixed4 _LineColor;
			float _LineSize;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float depthAlpha : TEXCOORD2;
};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = WireLineVert(v.vertex);
				o.uv = v.uv;
				o.depthAlpha = DepthAlpha(v.vertex); // distance from vertex to camera.
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float lineAlpha = WireLineAlpha(i.uv, _LineSize);
				lineAlpha *= _LineColor.a;
			    return fixed4(_LineColor.rgb, lineAlpha * i.depthAlpha);
			}
			ENDCG
		}
	}
}
