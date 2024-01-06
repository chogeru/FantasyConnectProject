using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyData用のScriptableObjectを作成します
[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data", order = 1)]
public class EnemyData : ScriptableObject
{
    // レイキャストに関するパラメーター
    [Header("レイキャスト")]
    [SerializeField, Header("レイキャスト距離")]
    private float m_RaycastDistance = 10f;
    [SerializeField, Header("レイキャスト角度")]
    private float m_Angle = 90f;
    [SerializeField, Header("レイの数")]
    private int rayCount = 180;

    // 敵のパラメーター
    [SerializeField, Header("追跡対象のタグ")]
    private string m_TargetTag = "Player";
    [SerializeField, Header("最大速度")]
    private float m_MaxSpeed = 5f;
    [SerializeField, Header("加速値")]
    private float m_Acceleration = 2f;
    [SerializeField, Header("攻撃距離")]
    private float attackDistance = 5f;
    [SerializeField, Header("回転速度")]
    private float m_RotationSpeed = 5f;
    [SerializeField,Header("最大体力")]
    public int m_MaxHp;
    [SerializeField, Header("ドロップする金額")]
    private int m_DropMony = 500;

    // オーディオ関連
    [Header("オーディオ")]
    [SerializeField, Header("ヒットボイスクリップ")]
    private AudioClip m_HitVoiceClip;
    [SerializeField, Header("死亡時ボイスクリップ")]
    private AudioClip m_DieVoiceClip;
    [SerializeField, Header("被弾SE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("死亡SE")]
    private AudioClip m_DieSEClip;
    [SerializeField, Header("足音")]
    private AudioClip m_FootStepClip;

}



