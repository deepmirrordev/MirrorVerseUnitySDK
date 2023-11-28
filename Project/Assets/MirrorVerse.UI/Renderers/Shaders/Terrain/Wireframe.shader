Shader "MirrorVerse/Wireframe"
{
	Properties
	{
	    _LineColor ("Line color", Color) = (0, 0, 0, 1)
	    _LineSize ("Line size", float) = 0.3
	}
	
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			CGPROGRAM
			#include "UnityCG.cginc"
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
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = WireLineVert(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float lineAlpha = WireLineAlpha(i.uv, _LineSize);
				lineAlpha *= _LineColor.a;
				return fixed4(_LineColor.rgb, lineAlpha);
			}
			ENDCG
		}
	}
}
