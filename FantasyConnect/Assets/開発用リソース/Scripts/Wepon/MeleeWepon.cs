using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class MeleeWepon : MonoBehaviour
{

    [SerializeField, Header("‹ßÚUŒ‚—pƒRƒ‰ƒCƒ_")]
    private GameObject m_MeleeWeponCol;
    #region
    [SerializeField, Header("‹ßÚÅ‘åUŒ‚—Í")]
    private int m_MaxAttack;
    [SerializeField, Header("Å¬UŒ‚—Í")]
    private int m_MinAttack;
    #endregion
    [SerializeField, Header("UŒ‚ƒN[ƒ‹ƒ^ƒCƒ€")]
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
            m_PlayerAnimator.SetBool("UŒ‚", false);
        }
    }
    private void Attck()
    {
        StopMoveAnime();
        m_PlayerAnimator.SetBool("UŒ‚", true);
    }

    private void StopMoveAnime()
    {
        m_PlayerAnimator.SetBool("Idle", false);
        m_PlayerAnimator.SetBool("Walk", false);
        m_PlayerAnimator.SetBool("Run", false);
    }
}
