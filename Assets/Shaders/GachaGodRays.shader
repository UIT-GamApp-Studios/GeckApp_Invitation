Shader "UI/GachaGodRays"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _RayColor ("Ray Color", Color) = (1, 0.85, 0.4, 1)
        _RayCount ("Ray Count", Range(4, 64)) = 16
        _RaySharpness ("Ray Sharpness", Range(1, 30)) = 8
        _RotationSpeed ("Rotation Speed (vong/giay)", Range(-2, 2)) = 0.06
        _Intensity ("Intensity", Range(0, 3)) = 0.6

        _CenterX ("Center X (0-1)", Range(0,1)) = 0.5
        _CenterY ("Center Y (0-1)", Range(0,1)) = 0.5
        _InnerRadius ("Inner Radius (vung sat tam khong tia)", Range(0, 1)) = 0.08
        _OuterRadius ("Outer Radius (tia mo dan het o day)", Range(0.1, 2)) = 0.9
        _Aspect ("Aspect Ratio (width/height, set tu script)", Float) = 1.0

        _PulseIntensity ("Pulse Intensity", Range(0, 2)) = 0.25
        _PulseSpeed ("Pulse Speed", Range(0.1, 5)) = 0.6

        _Boost ("Boost Multiplier (dieu khien tu script)", Range(0, 5)) = 1.0

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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        // Additive: tia sáng CỘNG thêm ánh sáng vào những gì phía sau (lớp tối), không che mất gì
        Blend One One
        ColorMask [_ColorMask]

        Pass
        {
            Name "GachaGodRays"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float4 _MainTex_ST;

            fixed4 _RayColor;
            float _RayCount;
            float _RaySharpness;
            float _RotationSpeed;
            float _Intensity;

            float _CenterX;
            float _CenterY;
            float _InnerRadius;
            float _OuterRadius;
            float _Aspect;

            float _PulseIntensity;
            float _PulseSpeed;

            float _Boost;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                #ifdef UNITY_UI_CLIP_RECT
                float clipAlpha = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #else
                float clipAlpha = 1.0;
                #endif

                float t = _Time.y;

                // Toạ độ tương đối so với tâm, hiệu chỉnh theo tỉ lệ khung hình để tia tròn đều (không bị méo)
                float2 uv = IN.texcoord;
                float2 d = float2((uv.x - _CenterX) * _Aspect, uv.y - _CenterY);
                float dist = length(d);
                float theta = atan2(d.y, d.x);

                // Các tia xoay dần theo thời gian
                float spin = t * _RotationSpeed * 6.28318530718;
                float raySignal = sin(theta * _RayCount + spin) * 0.5 + 0.5;
                float rayShape = pow(raySignal, _RaySharpness);

                // Mờ dần: không có tia sát tâm (tránh chói gắt) và mờ hẳn ở bán kính ngoài
                float innerFade = smoothstep(_InnerRadius, _InnerRadius + 0.12, dist);
                float outerFade = 1.0 - smoothstep(_OuterRadius - 0.25, _OuterRadius, dist);
                float radialMask = saturate(innerFade * outerFade);

                // Nhấp nháy nhẹ theo nhịp
                float pulse = 1.0 + _PulseIntensity * (0.5 + 0.5 * sin(t * _PulseSpeed * 3.14159));

                float strength = rayShape * radialMask * _Intensity * pulse * _Boost * clipAlpha;
                strength *= IN.color.a; // cho phép script fade in/out qua alpha của Image

                float3 finalRGB = _RayColor.rgb * strength;

                return fixed4(finalRGB, strength);
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}
