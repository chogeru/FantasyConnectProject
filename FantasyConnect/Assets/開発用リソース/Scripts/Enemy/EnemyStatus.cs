using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyStatus : ScriptableObject
{
    public List<EnemyState> enemyStatusesList = new List<EnemyState>();

    [System.Serializable]
    public class EnemyState
    {
        [SerializeField,Header("�G�l�~�[�̖��O")]
        private string m_EnemyName;

        #region//�G�̊�{�X�e�[�^�X
        [SerializeField, Header("�ő�̗�")]
        private int m_MaxHp;
        [SerializeField, Header("�ő�U����")]
        private int m_MaxAttck;
        [SerializeField, Header("�ŏ��U����")]
        private int m_MinAttack;
        [SerializeField, Header("�h���")]
        private int m_Defens;
        [SerializeField, Header("�o���l")]
        private int m_Exp;
        #endregion
        public Animator animator;
        #region//�A�j���[�V�����̖��O
        [SerializeField]
        private string m_AttackAnime;
        [SerializeField]
        private string m_WalkAnime;
        [SerializeField]
        private string m_IdleAnime;
        [SerializeField]
        private string m_DieAnime;
        [SerializeField]
        private string m_isGetHit;
        #endregion
        #region �^�C�v
        [SerializeField]
        private EnemyButtleType m_CurrentButtleType;
        [SerializeField]
        private EnemyStateType m_CurrentState;
        [SerializeField]
        public enum EnemyStateType
        {
            Idle,
            Walk,
            Buttle,
            Hit,
            Die,
            Attack
        }

        public enum EnemyButtleType
        {
            Melee, // �ߐڃ^�C�v
            Magic //���@�^�C�v
        }
        #endregion
        public int MaxHp { get => m_MaxHp; set => m_MaxHp = value; }
        public int MaxAttack { get => m_MaxAttck; set => m_MaxAttck = value; }
        public int MinAttack { get => m_MinAttack; set => m_MinAttack = value; }
        public int Defense { get => m_Defens; set => m_Defens = value; }
        public int EXP { get => m_Exp; set => m_Exp = value; }
        public EnemyButtleType ButtleType { get => m_CurrentButtleType; set => m_CurrentButtleType = value; }
        public EnemyStateType StateType { get => m_CurrentState; set => m_CurrentState = value; }
        public string EnemyName { get => m_EnemyName; set => m_EnemyName = value; }
    }
}



