using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyData�p��ScriptableObject���쐬���܂�
[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("���C�L���X�g�Ɋւ���p�����[�^�[")]
    [SerializeField, Header("���C�L���X�g����")]
    private float m_RaycastDistance = 10f;
    public float RaycastDistance
    {
        get { return m_RaycastDistance; }
        set { m_RaycastDistance = value; }
    }

    [SerializeField, Header("���C�L���X�g�p�x")]
    private float m_Angle = 90f;
    public float Angle
    {
        get { return m_Angle; }
        set { m_Angle = value; }
    }

    [SerializeField, Header("���C�̐�")]
    private int m_RayCount = 180;
    public int RayCount
    {
        get { return m_RayCount; }
        set { m_RayCount = value; }
    }

    [Header("�G�̃p�����[�^�[")]
    [SerializeField, Header("�ǐՑΏۂ̃^�O")]
    private string m_TargetTag = "Player";
    public string TargetTag
    {
        get { return m_TargetTag; }
        set { m_TargetTag = value; }
    }

    [SerializeField, Header("�ő呬�x")]
    private float m_MaxSpeed = 5f;
    public float MaxSpeed
    {
        get { return m_MaxSpeed; }
        set { m_MaxSpeed = value; }
    }

    [SerializeField, Header("�����l")]
    private float m_Acceleration = 2f;
    public float Acceleration
    {
        get { return m_Acceleration; }
        set { m_Acceleration = value; }
    }

    [SerializeField, Header("�U������")]
    private float m_AttackDistance = 5f;
    public float AttackDistance
    {
        get { return m_AttackDistance; }
        set { m_AttackDistance = value; }
    }

    [SerializeField, Header("��]���x")]
    private float m_RotationSpeed = 5f;
    public float RotationSpeed
    {
        get { return m_RotationSpeed; }
        set { m_RotationSpeed = value; }
    }

    [SerializeField, Header("�ő�̗�")]
    private int m_MaxHp;
    public int MaxHp
    {
        get { return m_MaxHp; }
        set { m_MaxHp = value; }
    }
    [SerializeField, Header("�d��")]
    private float m_Gravity = 9.81f;
    public float Gravity
    {
        get { return m_Gravity; }
        set { m_Gravity = value; }
    }

    [SerializeField, Header("�h���b�v������z")]
    private int m_DropMony = 500;
    public int DropMony
    {
        get { return m_DropMony; }
        set { m_DropMony = value; }
    }

    [Header("�I�[�f�B�I�֘A")]
    [SerializeField, Header("�q�b�g�{�C�X�N���b�v")]
    private AudioClip m_HitVoiceClip;
    public AudioClip HitVoiceClip
    {
        get { return m_HitVoiceClip; }
        set { m_HitVoiceClip = value; }
    }

    [SerializeField, Header("���S���{�C�X�N���b�v")]
    private AudioClip m_DieVoiceClip;
    public AudioClip DieVoiceClip
    {
        get { return m_DieVoiceClip; }
        set { m_DieVoiceClip = value; }
    }

    [SerializeField, Header("��eSE")]
    private AudioClip m_HitSEClip;
    public AudioClip HitSEClip
    {
        get { return m_HitSEClip; }
        set { m_HitSEClip = value; }
    }

    [SerializeField, Header("���SSE")]
    private AudioClip m_DieSEClip;
    public AudioClip DieSEClip
    {
        get { return m_DieSEClip; }
        set { m_DieSEClip = value; }
    }

    [SerializeField, Header("����")]
    private AudioClip m_FootStepClip;
    public AudioClip FootStepClip
    {
        get { return m_FootStepClip; }
        set { m_FootStepClip = value; }
    }
}



