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
        [SerializeField,Header("エネミーの名前")]
        private string m_EnemyName;

        #region//敵の基本ステータス
        [SerializeField, Header("最大体力")]
        private int m_MaxHp;
        [SerializeField, Header("最大攻撃力")]
        private int m_MaxAttck;
        [SerializeField, Header("最小攻撃力")]
        private int m_MinAttack;
        [SerializeField, Header("防御力")]
        private int m_Defens;
        [SerializeField, Header("経験値")]
        private int m_Exp;
        #endregion
        public Animator animator;
        #region//アニメーションの名前
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
        #region タイプ
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
            Melee, // 近接タイプ
            Magic //魔法タイプ
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



