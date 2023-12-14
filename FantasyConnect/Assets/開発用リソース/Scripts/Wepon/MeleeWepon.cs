using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class MeleeWepon : MonoBehaviour
{

    [SerializeField, Header("�ߐڍU���p�R���C�_")]
    private GameObject m_MeleeWeponCol;
    #region
    [SerializeField, Header("�ߐڍő�U����")]
    private int m_MaxAttack;
    [SerializeField, Header("�ŏ��U����")]
    private int m_MinAttack;
    #endregion
    [SerializeField, Header("�U���N�[���^�C��")]
    private float m_AttackCoolTime;

    [SerializeField]
    private Animator m_PlayerAnimator;
    void Start()
    {

        m_MeleeWeponCol = GameObject.Find("MeleeCol");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attck();
        }
        else
        {
            m_PlayerAnimator.SetBool("�U��", false);
        }
    }
    private void Attck()
    {
        StopMoveAnime();
        m_PlayerAnimator.SetBool("�U��", true);
    }

    private void StopMoveAnime()
    {
        m_PlayerAnimator.SetBool("Idle", false);
        m_PlayerAnimator.SetBool("Walk", false);
        m_PlayerAnimator.SetBool("Run", false);
    }
}
