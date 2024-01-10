////By Hayashi 
Shader "UI/BloomImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BloomIntensity("Bloom Intensity", Range(1, 10)) = 1.0// Bloom����
        _ScrollSpeed("Scroll Speed", Range(0, 10)) = 1.0// �摜���X�N���[�����鑬��
    }
    SubShader
    {
// �`��̃^�O
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata_t
{
    float4 vertex : POSITION; // �ʒu���
    float2 uv : TEXCOORD0; // �e�N�X�`���̍��W���
};

struct v2f
{
    float2 uv : TEXCOORD0; // �]������e�N�X�`���̍��W���
                UNITY_FOG_COORDS(1)// �t�H�O���W
    float4 vertex : SV_POSITION; // �V�X�e���o�[�e�b�N�X���W
};

sampler2D _MainTex; // �e�N�X�`�����T���v�����O���邽�߂̕ϐ�
float _BloomThreshold;
float _BloomIntensity;
float _ScrollSpeed;

v2f vert(appdata_t v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex); // �I�u�W�F�N�g���W���N���b�v���W�ɕϊ��I
    o.uv = v.uv; // �e�N�X�`���̍��W�����R�s�[
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    // ���ԂƃX�N���[���̑�������I�t�Z�b�g���v�Z����
    float xOffset = _Time.x * _ScrollSpeed;

    // �I�t�Z�b�g�̐������Ə��������v�Z����
    float intPart;
    float fracPart = modf(xOffset, intPart);

    // �e�N�X�`���ɃX�N���[����K�p����i���[�v����悤�Ɂj
    float2 wrappedUV = i.uv;
    wrappedUV.x += fracPart;
    if (wrappedUV.x > 1.0)
    {
        wrappedUV.x -= 1.0;
    }

    // �e�N�X�`�����T���v�����O����
    fixed4 col = tex2D(_MainTex, wrappedUV);

      // Bloom�G�t�F�N�g��K�p
    float bloom = saturate(col.a - _BloomThreshold);
    col *= _BloomIntensity * bloom;

    //CutOff����ۂ͌X�̐��l��������
    //����0.5�ȉ��̕����̓I�t��
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