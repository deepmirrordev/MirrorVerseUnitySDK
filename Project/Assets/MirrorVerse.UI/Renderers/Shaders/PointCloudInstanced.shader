Shader "MirrorVerse/PointCloudInstanced" {
    SubShader{
        
        Cull Back

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct appdata_t
        {
            float4 positionOS: POSITION;
            float4 color: COLOR;
        };

        struct v2f
        {
            float4 positionCS: SV_POSITION;
            half4 color: COLOR;
        };

        struct MeshProperties {
            float4x4 transform;
            float4 color;
            int batch;
            float enterTime;
            float exitTime;
        };

        StructuredBuffer<MeshProperties> _properties;
        float3 _cameraPosition;
        float _displayDistance;
        float _fadeInDuration;
        float _fadeOutDuration;
        float _inflationScale;
        float _opacity;

        float GetDistance2D(float4 vertexPos)
        {
            // Only get 2D distance
            float2 pointPos = float2(vertexPos[0], vertexPos[2]);
            float2 cameraPos = float2(_cameraPosition[0], _cameraPosition[2]);
            float dist = distance(cameraPos, pointPos);
            return dist;
        }
        
        float4x4 GetScaleMatrix(float s)
        {
            return float4x4(s, 0, 0, 0,
                            0, s, 0, 0,
                            0, 0, s, 0,
                            0, 0, 0, 1);
        }

        float4 GetScaleAccordingTime(MeshProperties prop)
        {
            float scale = 1;

            // Fading in.
            if (_Time.y - prop.enterTime < _fadeInDuration) {
                float m = (_Time.y - prop.enterTime) / _fadeInDuration;
                scale = 0.2 + m * 0.8;
                return scale;
            }

            float inflated = 1;
            if (_Time.y - prop.enterTime - _fadeInDuration < 6.0) {
                float m = (_Time.y - prop.enterTime - _fadeInDuration) / 6.0;
                inflated = 1.0 + m * (_inflationScale - 1.0);
            }
            else {
                inflated = _inflationScale;
            }

            // Fading out.
            if (prop.exitTime > 0) {
                if (_Time.y - prop.exitTime < _fadeOutDuration) {
                    float m = (_Time.y - prop.exitTime) / _fadeOutDuration;
                    scale = inflated * (1 - m);
                }
                else {
                    scale = 0;
                }
                return scale;
            }

            // Inflate.
            if (prop.exitTime <= 0) {
                scale = inflated;
            }
            return scale;
        }

        float4 GetColorAccordingTime(MeshProperties prop)
        {
            // Fading in.
            if (_Time.y - prop.enterTime < _fadeInDuration) {
                float m = (_Time.y - prop.enterTime) / _fadeInDuration;
                prop.color[3] = 0.1 + m * (_opacity - 0.1);
                return prop.color;
            }

            // Fading out.
            if (prop.exitTime > 0) {
                if (_Time.y - prop.exitTime < _fadeOutDuration) {
                    float n = (_Time.y - prop.exitTime) / _fadeOutDuration;
                    prop.color[3] = (1 - n) * _opacity;
                    return prop.color;
                }
            }

            prop.color[3] = _opacity;
            return prop.color;
        }

        float4 GetColorAccordingDistance(float dist, float4 color)
        {
            if (dist > _displayDistance) {
                color[3] = 0;
            }
            else if (dist > 0.25 * _displayDistance) {
                float m = (_displayDistance - dist) / (0.25 * _displayDistance);
                color[3] = color[3] * m;
            }
            return color;
        }

        ENDHLSL

        Pass
        {
            Name "PC_COLOR_ALPHA"
            Tags {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGBA

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID)
            {
                v2f o;

                MeshProperties prop = _properties[instanceID];

                float4 scale = GetScaleAccordingTime(prop);
                float4 scaledPos = mul(i.positionOS, GetScaleMatrix(scale));
                float4 pos = mul(prop.transform, scaledPos);
                VertexPositionInputs vertexPositionInputs = GetVertexPositionInputs(pos);
                o.positionCS = vertexPositionInputs.positionCS;

                float dist = GetDistance2D(pos);
                o.color = GetColorAccordingDistance(dist, GetColorAccordingTime(prop));
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}
