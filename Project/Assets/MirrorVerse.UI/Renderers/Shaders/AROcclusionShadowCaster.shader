Shader "MirrorVerse/AR Occlusion Shadow Caster"
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

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            Cull Back

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment      
            #pragma shader_feature _ALPHATEST_ON

            // GPU Instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"   

            CBUFFER_START(UnityPerMaterial)   
                half4 _TintColor;   
                sampler2D _MainTex;   
                float4 _MainTex_ST;   
                float _Alpha;   
            CBUFFER_END

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;

                #if _ALPHATEST_ON  
                    float2 uv : TEXCOORD0;  
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;  
                #if _ALPHATEST_ON  
                    float2 uv : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO 
            };

            VertexOutput ShadowPassVertex(VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normal.xyz);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                o.vertex = positionCS;   
                #if _ALPHATEST_ON   
                    o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; ;  
                #endif
                return o;
            }
            half4 ShadowPassFragment(VertexOutput i) : SV_TARGET
            { 
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);    
                #if _ALPHATEST_ON    
                    float4 col = tex2D(_MainTex, i.uv);
                    clip(col.a - _Alpha);    
                #endif
                return 0;
            }
            ENDHLSL
        }
        

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Hidden/InternalErrorShader"
}
