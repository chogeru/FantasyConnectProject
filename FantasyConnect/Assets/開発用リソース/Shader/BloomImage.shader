////By Hayashi 
Shader "UI/BloomImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BloomIntensity("Bloom Intensity", Range(1, 10)) = 1.0// Bloom強さ
        _ScrollSpeed("Scroll Speed", Range(0, 10)) = 1.0// 画像がスクロールする速さ
    }
    SubShader
    {
// 描画のタグ
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata_t
{
    float4 vertex : POSITION; // 位置情報
    float2 uv : TEXCOORD0; // テクスチャの座標情報
};

struct v2f
{
    float2 uv : TEXCOORD0; // 転送するテクスチャの座標情報
                UNITY_FOG_COORDS(1)// フォグ座標
    float4 vertex : SV_POSITION; // システムバーテックス座標
};

sampler2D _MainTex; // テクスチャをサンプリングするための変数
float _BloomThreshold;
float _BloomIntensity;
float _ScrollSpeed;

v2f vert(appdata_t v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex); // オブジェクト座標をクリップ座標に変換！
    o.uv = v.uv; // テクスチャの座標情報をコピー
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    // 時間とスクロールの速さからオフセットを計算する
    float xOffset = _Time.x * _ScrollSpeed;

    // オフセットの整数部と小数部を計算する
    float intPart;
    float fracPart = modf(xOffset, intPart);

    // テクスチャにスクロールを適用する（ループするように）
    float2 wrappedUV = i.uv;
    wrappedUV.x += fracPart;
    if (wrappedUV.x > 1.0)
    {
        wrappedUV.x -= 1.0;
    }

    // テクスチャをサンプリングする
    fixed4 col = tex2D(_MainTex, wrappedUV);

      // Bloomエフェクトを適用
    float bloom = saturate(col.a - _BloomThreshold);
    col *= _BloomIntensity * bloom;

    //CutOffする際は個々の数値をいじる
    //現在0.5以下の部分はオフに
    if (col.a <= 0.5)
    {
        discard;
    }

    return col;
}
            ENDCG
        }
    }
}