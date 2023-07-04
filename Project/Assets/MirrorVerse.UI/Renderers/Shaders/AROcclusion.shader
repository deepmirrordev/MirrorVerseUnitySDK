Shader "MirrorVerse/AR Occlusion"
{
    Properties
    {
        _ShadowColor ("Shadow Color", Color) = (0.35,0.4,0.45,1.0)
    }

    SubShader
    {   
        Tags{
            "RenderPipeline" = "UniversalPipeline"

        }

        Pass
        {
            Name "PlaneOcclusion"
            Tags {
                "RenderType"="Opaque" 
                "Queue" = "Geometry-1"
            }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                return fixed4(0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Hidden/InternalErrorShader"
}
