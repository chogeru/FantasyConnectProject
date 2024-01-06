using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyData�p��ScriptableObject���쐬���܂�
[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data", order = 1)]
public class EnemyData : ScriptableObject
{
    // ���C�L���X�g�Ɋւ���p�����[�^�[
    [Header("���C�L���X�g")]
    [SerializeField, Header("���C�L���X�g����")]
    private float m_RaycastDistance = 10f;
    [SerializeField, Header("���C�L���X�g�p�x")]
    private float m_Angle = 90f;
    [SerializeField, Header("���C�̐�")]
    private int rayCount = 180;

    // �G�̃p�����[�^�[
    [SerializeField, Header("�ǐՑΏۂ̃^�O")]
    private string m_TargetTag = "Player";
    [SerializeField, Header("�ő呬�x")]
    private float m_MaxSpeed = 5f;
    [SerializeField, Header("�����l")]
    private float m_Acceleration = 2f;
    [SerializeField, Header("�U������")]
    private float attackDistance = 5f;
    [SerializeField, Header("��]���x")]
    private float m_RotationSpeed = 5f;
    [SerializeField,Header("�ő�̗�")]
    public int m_MaxHp;
    [SerializeField, Header("�h���b�v������z")]
    private int m_DropMony = 500;

    // �I�[�f�B�I�֘A
    [Header("�I�[�f�B�I")]
    [SerializeField, Header("�q�b�g�{�C�X�N���b�v")]
    private AudioClip m_HitVoiceClip;
    [SerializeField, Header("���S���{�C�X�N���b�v")]
    private AudioClip m_DieVoiceClip;
    [SerializeField, Header("��eSE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("���SSE")]
    private AudioClip m_DieSEClip;
    [SerializeField, Header("����")]
    private AudioClip m_FootStepClip;

}



