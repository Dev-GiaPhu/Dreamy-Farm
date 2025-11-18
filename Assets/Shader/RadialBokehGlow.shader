Shader "Custom/RadialBokehGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurStrength ("Blur Strength", Range(0, 2)) = 0.5
        _FocusPos ("Focus Position", Vector) = (0.5, 0.5, 0, 0)
        _FocusRadius ("Focus Radius", Range(0, 1)) = 0.25
        _GlowIntensity ("Glow Intensity", Float) = 2.0
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _BokehShape ("Bokeh Shape (3=tri,6=hex)", Range(3, 8)) = 6
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurStrength;
            float4 _FocusPos;
            float _FocusRadius;
            float _GlowIntensity;
            float4 _GlowColor;
            float _BokehShape;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Tạo mẫu bokeh theo hình tròn/lục giác
            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = _FocusPos.xy;
                float dist = distance(i.uv, center);
                float blurAmount = saturate((dist - _FocusRadius) / (1.0 - _FocusRadius)) * _BlurStrength;

                float2 texel = _MainTex_TexelSize.xy * blurAmount * 20;

                fixed4 color = 0;
                int samples = 0;

                // Tạo hình lục giác (bokeh)
                for (int s = 0; s < (int)_BokehShape; s++)
                {
                    float angle = UNITY_PI * 2.0 * (s / _BokehShape);
                    float2 dir = float2(cos(angle), sin(angle));
                    for (float r = 0.2; r <= 1.0; r += 0.2)
                    {
                        color += tex2D(_MainTex, i.uv + dir * texel * r);
                        samples++;
                    }
                }
                color /= samples;

                // Ánh sáng lung linh
                float brightness = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float glow = pow(brightness, 2.5) * _GlowIntensity;

                color.rgb += _GlowColor.rgb * glow;
                return color;
            }
            ENDCG
        }
    }
}
