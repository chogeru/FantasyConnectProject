Shader "Custom/DissolveShader"
{
 Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.5
        _TintColor ("Tint Color", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _NormalStrength ("Normal Strength", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _DissolveMap;
            sampler2D _NormalMap;
            float _DissolveAmount;
            fixed4 _TintColor;
            float _Brightness;
            float _NormalStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dissolveValue = tex2D(_DissolveMap, i.uv).r;

                if (dissolveValue < _DissolveAmount)
                {
                    discard;
                }

                float3 normalMap = UnpackNormal(tex2D(_NormalMap, i.uv));

                float3 worldNormal = mul((float3x3)unity_ObjectToWorld, normalMap * 2.0 - 1.0);
                worldNormal = normalize(worldNormal) * _NormalStrength; // Apply normal strength

                fixed4 col = tex2D(_MainTex, i.uv) * _TintColor;
                col.rgb *= _Brightness; // Apply brightness

                return col;
            }
            ENDCG
        }
    }
    
}