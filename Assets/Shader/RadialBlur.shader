Shader "Custom/RadialBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurStrength ("Blur Strength", Range(0, 2)) = 0.7
        _FocusPos ("Focus Position", Vector) = (0.5, 0.5, 0, 0)
        _FocusRadius ("Focus Radius", Range(0, 10)) = 0.25
        _GlowIntensity ("Glow Intensity", Range(0, 100)) = 1.2
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = _FocusPos.xy;
                float dist = distance(i.uv, center);

                // Mức blur theo khoảng cách
                float blurAmount = saturate((dist - _FocusRadius) / (1.0 - _FocusRadius)) * _BlurStrength;

                // Nếu rất gần trung tâm -> giữ nét
                if (blurAmount <= 0.001)
                    return tex2D(_MainTex, i.uv);

                // Gaussian blur 32 mẫu cho mịn, không vỡ
                float2 texel = _MainTex_TexelSize.xy * blurAmount * 10.0;
                fixed4 col = 0;
                float weightSum = 0;

                // Gaussian weights (đối xứng)
                float weights[9] = {0.05,0.09,0.12,0.15,0.18,0.15,0.12,0.09,0.05};

                for (int x = -4; x <= 4; x++)
                {
                    for (int y = -4; y <= 4; y++)
                    {
                        float w = weights[abs(x)] * weights[abs(y)];
                        col += tex2D(_MainTex, i.uv + texel * float2(x, y)) * w;
                        weightSum += w;
                    }
                }

                col /= weightSum;

                // Áp dụng glow màu tùy chỉnh
                float glowPower = pow(blurAmount, 1.5) * _GlowIntensity;
                col.rgb += _GlowColor.rgb * glowPower * col.rgb;

                return col;
            }
            ENDCG
        }
    }
}
