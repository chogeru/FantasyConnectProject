using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicWepon : MonoBehaviour
{
    enum eAttckType
    {
        Bullet,
        RangeAttack
    }
    [SerializeField]
    private eAttckType attckType;
    [SerializeField, Header("攻撃クールタイム")]
    private float m_AttackCoolTime;
    [SerializeField]
    private Animator m_PlayerAnimator;
    private bool isAttck=false;
    [SerializeField]
    private GameObject m_MagicBullet;
    [SerializeField]
    private GameObject m_RangeAttckCol;


    void Update()
    {
        m_AttackCoolTime += Time.deltaTime;
        if(m_AttackCoolTime>1)
        {
            isAttck = true;
            m_AttackCoolTime = 0;
        }
        else
        {
            isAttck=false;
        }
        if(Input.GetMouseButton(0))
        {
            switch(attckType)
            {
                case eAttckType.Bullet:
                    MagicBulletAttack();
                    break;
                    case eAttckType.RangeAttack:
                    MagicRangeAttack();
                    break;
                    default: 

                    break;
            }
        }
        else
        {
            m_PlayerAnimator.SetBool("MagicAttck", false);
            m_RangeAttckCol.SetActive(false);
        }
    }
    private void MagicBulletAttack()
    {
        StopMoveAnime();
        m_PlayerAnimator.SetBool("MagicAttck", true);
        if (isAttck)
        {
            Instantiate(m_MagicBullet, transform.position, Quaternion.identity);
            m_AttackCoolTime = 0;

        }
    }
    private void MagicRangeAttack()
    {
        StopMoveAnime();
        m_PlayerAnimator.SetBool("MagicAttck", true);
        if (isAttck)
        {
            m_RangeAttckCol.SetActive(true);
            Instantiate(m_MagicBullet, transform.position, Quaternion.identity);
            m_AttackCoolTime = 0;

        }
    }


    private void StopMoveAnime()
    {
        m_PlayerAnimator.SetBool("Idle", false);
        m_PlayerAnimator.SetBool("Walk", false);
        m_PlayerAnimator.SetBool("Run", false);
    }
}
