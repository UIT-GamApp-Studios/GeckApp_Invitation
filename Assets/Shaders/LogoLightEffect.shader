Shader "UI/LogoLightEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 0.6
        _GlowPower ("Glow Power", Range(0.1,5)) = 1.5
        _GlowSize ("Glow Size", Range(0,0.2)) = 0.012
        
        _SweepColor ("Sweep Color", Color) = (1,1,1,1)
        _SweepHighlight ("Sweep Highlight", Color) = (1,0.8,0.4,1)
        _SweepWidth ("Sweep Width", Range(0.05,1)) = 0.08
        _SweepIntensity ("Sweep Intensity", Range(0,5)) = 1.5
        _SweepSoftness ("Sweep Softness", Range(0.01,0.5)) = 0.04
        _SweepSpeed ("Sweep Speed", Range(0.1,5)) = 1.0
        _SweepDirection ("Sweep Direction (0=Ngang trai-phai, 1=Cheo duoi trai len tren phai)", Range(0,1)) = 0
        
        _PulseIntensity ("Pulse Intensity", Range(0,2)) = 0.15
        _PulseSpeed ("Pulse Speed", Range(0.1,5)) = 1.0
        
        _SecondaryColor ("Secondary Color", Color) = (1,0.5,0.2,1)
        _SecondaryIntensity ("Secondary Intensity", Range(0,2)) = 0.25
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            Name "LogoLightEffect"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
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
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowPower;
            float _GlowSize;
            
            fixed4 _SweepColor;
            fixed4 _SweepHighlight;
            float _SweepWidth;
            float _SweepIntensity;
            float _SweepSoftness;
            float _SweepSpeed;
            float _SweepDirection;
            
            float _PulseIntensity;
            float _PulseSpeed;
            
            fixed4 _SecondaryColor;
            float _SecondaryIntensity;
            
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
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // Alpha clip / rect clip
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                
                // Only apply effects to visible (non-transparent) pixels
                float alpha = color.a;
                if (alpha < 0.01) discard;
                
                // UV center for sweep
                float2 uv = IN.texcoord;
                float t = _Time.y;
                
                // Sweep position moves along the sweep axis, loops seamlessly
                float sweepPos = frac(t * _SweepSpeed);
                
                // Sweep axis: 0 = ngang (trai->phai theo uv.x), 1 = cheo (goc duoi trai -> goc tren phai)
                float horizontalCoord = uv.x;
                float diagonalCoord = saturate((uv.x + uv.y) * 0.5);
                float sweepCoord = lerp(horizontalCoord, diagonalCoord, _SweepDirection);
                
                // Narrow sweep - sharp light line with tight soft edges
                float distFromSweep = abs(sweepCoord - sweepPos);
                
                // Main narrow bright band - sharp falloff
                float sweep = 1.0 - smoothstep(_SweepWidth - _SweepSoftness, _SweepWidth, distFromSweep);
                // Sharper inner core
                float sweepCore = 1.0 - smoothstep(0.0, _SweepWidth * 0.5, distFromSweep);
                
                // Very sharp white center line
                float peak = exp(-distFromSweep * distFromSweep * 800.0);
                
                // Keep original logo color preserved (no modification)
                // Only add subtle effects around/within without overpowering
                float3 baseColor = color.rgb;
                
                // Subtle glow effect - sample slightly offset to create soft outer glow
                float3 glow = float3(0,0,0);
                float glowSize = _GlowSize;
                float2 offsets[8] = {
                    float2(-1,-1), float2(0,-1), float2(1,-1),
                    float2(-1, 0),               float2(1, 0),
                    float2(-1, 1), float2(0, 1), float2(1, 1)
                };
                
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    float2 offsetUV = IN.texcoord + offsets[i] * glowSize;
                    float sampleAlpha = tex2D(_MainTex, offsetUV).a * IN.color.a;
                    glow += _GlowColor.rgb * sampleAlpha;
                }
                glow /= 8.0;
                glow *= _GlowIntensity * pow(max(0.0, alpha), _GlowPower);
                
                // Sweep effect - narrow bright band, only modifies edges where sweep passes
                float sweepMask = sweep * alpha;
                float coreMask = sweepCore * alpha;
                float3 sweepEffect = _SweepColor.rgb * sweepMask * _SweepIntensity * 0.5;
                sweepEffect += _SweepHighlight.rgb * coreMask * _SweepIntensity * 0.4;
                sweepEffect += float3(1.0, 1.0, 1.0) * peak * alpha * _SweepIntensity * 0.6;
                
                // Subtle pulse
                float globalPulse = 0.5 + 0.5 * sin(t * _PulseSpeed * 3.14159);
                float3 pulseEffect = _GlowColor.rgb * _PulseIntensity * globalPulse * alpha * 0.15;
                
                // Combine: base color (preserved) + subtle additive effects
                // Use lighter blending so logo color stays mostly intact
                float3 finalRGB = baseColor;
                finalRGB = lerp(finalRGB, finalRGB + glow, 0.5);  // Gentle glow blend
                finalRGB += sweepEffect;
                finalRGB += pulseEffect;
                
                return fixed4(finalRGB, color.a);
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}