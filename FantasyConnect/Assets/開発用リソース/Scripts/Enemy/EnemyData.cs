using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyData用のScriptableObjectを作成します
[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("レイキャストに関するパラメーター")]
    [SerializeField, Header("レイキャスト距離")]
    private float m_RaycastDistance = 10f;
    public float RaycastDistance
    {
        get { return m_RaycastDistance; }
        set { m_RaycastDistance = value; }
    }

    [SerializeField, Header("レイキャスト角度")]
    private float m_Angle = 90f;
    public float Angle
    {
        get { return m_Angle; }
        set { m_Angle = value; }
    }

    [SerializeField, Header("レイの数")]
    private int m_RayCount = 180;
    public int RayCount
    {
        get { return m_RayCount; }
        set { m_RayCount = value; }
    }

    [Header("敵のパラメーター")]
    [SerializeField, Header("追跡対象のタグ")]
    private string m_TargetTag = "Player";
    public string TargetTag
    {
        get { return m_TargetTag; }
        set { m_TargetTag = value; }
    }

    [SerializeField, Header("最大速度")]
    private float m_MaxSpeed = 5f;
    public float MaxSpeed
    {
        get { return m_MaxSpeed; }
        set { m_MaxSpeed = value; }
    }

    [SerializeField, Header("加速値")]
    private float m_Acceleration = 2f;
    public float Acceleration
    {
        get { return m_Acceleration; }
        set { m_Acceleration = value; }
    }

    [SerializeField, Header("攻撃距離")]
    private float m_AttackDistance = 5f;
    public float AttackDistance
    {
        get { return m_AttackDistance; }
        set { m_AttackDistance = value; }
    }

    [SerializeField, Header("回転速度")]
    private float m_RotationSpeed = 5f;
    public float RotationSpeed
    {
        get { return m_RotationSpeed; }
        set { m_RotationSpeed = value; }
    }

    [SerializeField, Header("最大体力")]
    private int m_MaxHp;
    public int MaxHp
    {
        get { return m_MaxHp; }
        set { m_MaxHp = value; }
    }
    [SerializeField, Header("重力")]
    private float m_Gravity = 9.81f;
    public float Gravity
    {
        get { return m_Gravity; }
        set { m_Gravity = value; }
    }

    [SerializeField, Header("ドロップする金額")]
    private int m_DropMony = 500;
    public int DropMony
    {
        get { return m_DropMony; }
        set { m_DropMony = value; }
    }

    [Header("オーディオ関連")]
    [SerializeField, Header("ヒットボイスクリップ")]
    private AudioClip m_HitVoiceClip;
    public AudioClip HitVoiceClip
    {
        get { return m_HitVoiceClip; }
        set { m_HitVoiceClip = value; }
    }

    [SerializeField, Header("死亡時ボイスクリップ")]
    private AudioClip m_DieVoiceClip;
    public AudioClip DieVoiceClip
    {
        get { return m_DieVoiceClip; }
        set { m_DieVoiceClip = value; }
    }

    [SerializeField, Header("被弾SE")]
    private AudioClip m_HitSEClip;
    public AudioClip HitSEClip
    {
        get { return m_HitSEClip; }
        set { m_HitSEClip = value; }
    }

    [SerializeField, Header("死亡SE")]
    private AudioClip m_DieSEClip;
    public AudioClip DieSEClip
    {
        get { return m_DieSEClip; }
        set { m_DieSEClip = value; }
    }

    [SerializeField, Header("足音")]
    private AudioClip m_FootStepClip;
    public AudioClip FootStepClip
    {
        get { return m_FootStepClip; }
        set { m_FootStepClip = value; }
    }
}



