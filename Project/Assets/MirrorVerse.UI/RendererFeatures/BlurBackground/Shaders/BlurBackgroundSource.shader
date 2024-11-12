Shader "MirrorVerse/BlurBackgroundSource"
{
    HLSLINCLUDE
    #pragma target 3.0
    //HLSLcc is not used by default on gles
    #pragma prefer_hlslcc gles
    //SRP don't support dx9
    #pragma exclude_renderers d3d11_9x
    #pragma multi_compile_local _ PROCEDURAL_QUAD

    #ifdef SHADER_API_GLES
    #undef PROCEDURAL_QUAD
    #endif

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_MainTex);
    SAMPLER(sampler_MainTex);
    uniform half4 _MainTex_TexelSize;
    uniform half  _Radius;

    SAMPLER(sampler_LinearClamp);
    #define SAMPLE_SCREEN_TEX(tex, uv) SAMPLE_TEXTURE2D_X(tex, sampler_LinearClamp, UnityStereoTransformScreenSpaceTex(uv))

    struct v2f
    {
        half4 vertex : SV_POSITION;
        half4 texcoord : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };
    
    struct minimalVertexInput
    {
#if PROCEDURAL_QUAD
        uint vertexID  : SV_VertexID;
#else
        half4 position : POSITION;
#endif
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct minimalVertexOutput
    {
        half4 position : POSITION;
        half2 texcoord : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

#if defined(UNITY_SINGLE_PASS_STEREO)
    float4 UnityStereoAdjustedTexelSize(float4 texelSize)
    {
        texelSize.x = texelSize.x * 2.0; // texelSize.x = 1/w. For a double-wide texture, the true resolution is given by 2/w.
        texelSize.z = texelSize.z * 0.5; // texelSize.z = w. For a double-wide texture, the true size of the eye texture is given by w/2.
        return texelSize;
    }
#else
    float4 UnityStereoAdjustedTexelSize(float4 texelSize)
    {
        return texelSize;
    }
#endif

    void GetProceduralQuad(in uint vertexID, out float4 positionCS, out float2 uv)
    {
        positionCS = GetQuadVertexPosition(vertexID);
        positionCS.xy = positionCS.xy * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f);
        uv = GetQuadTexCoord(vertexID); // * _ScaleBias.xy + _ScaleBias.zw;
    }

    half2 VertexToUV(half2 vertex)
    {
        half2 texcoord = (vertex + 1.0) * 0.5; // triangle vert to uv
#if UNITY_UV_STARTS_AT_TOP
        texcoord = texcoord * half2(1.0, -1.0) + half2(0.0, 1.0);
#endif
        return texcoord;
    }

    v2f vert(minimalVertexInput v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        half4 pos;
        half2 uv;

#if PROCEDURAL_QUAD
        GetProceduralQuad(v.vertexID, pos, uv);
#else
        pos = v.position;
        uv = VertexToUV(v.position.xy);
#endif

        o.vertex = half4(pos.xy, 0.0, 1.0);

        half4 offset = half2(-0.5h, 0.5h).xxyy; //-x, -y, x, y
        offset *= UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xyxy;
        offset *= _Radius;
        o.texcoord = uv.xyxy + offset;

        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

        //Pray to the compiler god these will MAD
        half4 o = SAMPLE_SCREEN_TEX(_MainTex, i.texcoord.xw) / 4.0h;
        o += SAMPLE_SCREEN_TEX(_MainTex, i.texcoord.zw) / 4.0h;
        o += SAMPLE_SCREEN_TEX(_MainTex, i.texcoord.xy) / 4.0h;
        o += SAMPLE_SCREEN_TEX(_MainTex, i.texcoord.zy) / 4.0h;
        return o;
    }
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            //Crop before blur
            #pragma vertex vertCrop
            #pragma fragment frag

            half2 getNewUV(half2 oldUV)
            {
                // Crop region may be configured. Not support for now.
                half4 cropRegion = half4(0, 0, 1, 1);
                return lerp(cropRegion.xy, cropRegion.zw, oldUV);
            }

            v2f vertCrop(minimalVertexInput v)
            {
                v2f o = vert(v);

                o.texcoord.xy = getNewUV(o.texcoord.xy);
                o.texcoord.zw = getNewUV(o.texcoord.zw);

                return o;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
