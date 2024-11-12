// From https://github.com/MirzaBeig/Post-Processing-Scan
struct appdata_img_custom
{
    float4 vertex : POSITION;
    half2 texcoord : TEXCOORD0;

};

struct v2f_img_custom
{
    float4 pos : SV_POSITION;
    half2 uv : TEXCOORD0;
    half2 stereoUV : TEXCOORD2;
#if UNITY_UV_STARTS_AT_TOP
	half4 uv2 : TEXCOORD1;
	half4 stereoUV2 : TEXCOORD3;
#endif
    float4 ase_texcoord4 : TEXCOORD4;
};

uniform sampler2D _MainTex;
uniform half4 _MainTex_TexelSize;
uniform half4 _MainTex_ST;

uniform float4 _Colour;
uniform float _MultiplyBlend;
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
uniform float3 _Origin;
uniform float _Tiling;
uniform float _Speed;
uniform float _Power;
uniform float _MaskRadius;
uniform float _MaskHardness;
uniform float _MaskPower;
uniform float _StartTime;

uniform float _Depth;

float2 UnStereo(float2 UV)
{
#if UNITY_SINGLE_PASS_STEREO
	float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
	UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
#endif
    return UV;
}

float3 InvertDepthDir72_g1(float3 In)
{
    float3 result = In;
#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301
    result *= float3(1, 1, -1);
#endif
    return result;
}

v2f_img_custom vert_img_custom(appdata_img_custom v)
{
    v2f_img_custom o;
    float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
    float4 screenPos = ComputeScreenPos(ase_clipPos);
    o.ase_texcoord4 = screenPos;

    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = float4(v.texcoord.xy, 1, 1);

#if UNITY_UV_STARTS_AT_TOP
	o.uv2 = float4(v.texcoord.xy, 1, 1);
	o.stereoUV2 = UnityStereoScreenSpaceUVAdjust(o.uv2, _MainTex_ST);

	if (_MainTex_TexelSize.y < 0.0)
		o.uv.y = 1.0 - o.uv.y;
#endif
    o.stereoUV = UnityStereoScreenSpaceUVAdjust(o.uv, _MainTex_ST);
    return o;
}

half4 frag(v2f_img_custom i) : SV_Target
{
#ifdef UNITY_UV_STARTS_AT_TOP
	half2 uv = i.uv2;
	half2 stereoUV = i.stereoUV2;
#else
    half2 uv = i.uv;
    half2 stereoUV = i.stereoUV;
#endif	

    half4 finalColor;

    // ase common template code
    float2 uv_MainTex = i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    float4 ScreenColour19 = tex2D(_MainTex, uv_MainTex);
    float4 lerpResult158 = lerp(_Colour, (_Colour * ScreenColour19), _MultiplyBlend);
    float4 ScanColour88 = lerpResult158;
    float4 screenPos = i.ase_texcoord4;
    float4 ase_screenPosNorm = screenPos / screenPos.w;
    //ase_screenPosNorm = screenPos / _Depth;
    ase_screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
    float2 UV22_g3 = ase_screenPosNorm.xy;
    float2 localUnStereo22_g3 = UnStereo(UV22_g3);
    float2 break64_g1 = localUnStereo22_g3;
    float clampDepth69_g1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, ase_screenPosNorm.xy);
    //clampDepth69_g1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, ase_screenPosNorm.xy);
#ifdef UNITY_REVERSED_Z
    float staticSwitch38_g1 = (1.0 - clampDepth69_g1);
#else
    float staticSwitch38_g1 = clampDepth69_g1;
#endif
    float3 appendResult39_g1 = (float3(break64_g1.x, break64_g1.y, staticSwitch38_g1));
    float4 appendResult42_g1 = (float4((appendResult39_g1 * 2.0 + -1.0), 1.0));
    float4 temp_output_43_0_g1 = mul(unity_CameraInvProjection, appendResult42_g1);
    float3 temp_output_46_0_g1 = ((temp_output_43_0_g1).xyz / (temp_output_43_0_g1).w);
    float3 In72_g1 = temp_output_46_0_g1;
    float3 localInvertDepthDir72_g1 = InvertDepthDir72_g1(In72_g1);
    float4 appendResult49_g1 = (float4(localInvertDepthDir72_g1, 1.0));
    float SDF93 = length(distance(mul(unity_CameraToWorld, appendResult49_g1), float4(_Origin, 0.0)));
    float mulTime73 = (_Time.y - _StartTime) * _Speed;
    mulTime73 = clamp(mulTime73, 0, 1);
    float temp_output_141_0 = (_MaskRadius + 1.0);
    float lerpResult137 = lerp(0.0, (temp_output_141_0 - 0.001), _MaskHardness);
    float smoothstepResult134 = smoothstep(temp_output_141_0, lerpResult137, SDF93);
    float SDFMask107 = pow(smoothstepResult134, _MaskPower);
    float ColourAlpha160 = _Colour.a;
    float Scan77 = ((pow(frac(((SDF93 * _Tiling) - mulTime73)), _Power) * SDFMask107) * ColourAlpha160);
    float4 lerpResult17 = lerp(ScreenColour19, ScanColour88, Scan77);

    finalColor = lerpResult17;

    return finalColor;
}
