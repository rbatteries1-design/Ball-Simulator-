Shader "Custom/RainbowBall"
{
    Properties
    {
        _Speed ("Speed", Float) = 0.35
        _Scale ("Band Scale", Float) = 1.2
        _Saturation ("Saturation", Range(0, 1)) = 0.9
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _Gloss ("Gloss", Range(4, 256)) = 80
        _Rim ("Rim", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Speed;
            float _Scale;
            float _Saturation;
            float _Brightness;
            float _Gloss;
            float _Rim;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 objPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.objPos = v.vertex.xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float3 hue(float h)
            {
                return saturate(abs(frac(h + float3(1.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float h = frac(_Time.y * _Speed + (i.objPos.y + i.objPos.x * 0.5) * _Scale);
                float3 albedo = lerp(float3(1, 1, 1), hue(h), _Saturation) * _Brightness;

                float3 n = normalize(i.worldNormal);
                float3 l = normalize(float3(0.4, 0.8, 0.3));
                float3 v = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 hv = normalize(l + v);

                float diffuse = saturate(dot(n, l)) * 0.6 + 0.4;
                float spec = pow(saturate(dot(n, hv)), _Gloss) * 0.6;
                float rim = pow(1.0 - saturate(dot(n, v)), 3.0) * _Rim;

                float3 col = albedo * diffuse + spec + albedo * rim;
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
