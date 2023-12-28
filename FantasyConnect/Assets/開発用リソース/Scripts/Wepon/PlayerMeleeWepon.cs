using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMeleeWepon : MonoBehaviour
{
    [SerializeField, Header("攻撃用コライダー")]
    private GameObject m_AttckCol;
    [SerializeField, Header("プレイヤーのアニメーター")]
    private Animator m_PlayerAnimator;

    private void Update()
    {
        PlayerSystem playerSystem =GetComponent<PlayerSystem>();
        if(playerSystem != null)
        {
            if(playerSystem.isAttck)
            {
                MelleAttck();
            }
            if(playerSystem.isStrongAttck)
            {
                MelleAttck();
            }
            if (Input.GetMouseButtonUp(0))
            {
                playerSystem.isAttck = false;
                playerSystem.isStrongAttck = false;
            }
        }
      
    }
    private void MelleAttck()
    {
        m_AttckCol.SetActive(true);
    }

    private void MeleeAttckEnd()
    {
        m_AttckCol.SetActive(false);
        EndAttckAnimation();
        
    }
    private void EndAttckAnimation()
    {
        m_PlayerAnimator.SetBool("NormalAttack", false);
        m_PlayerAnimator.SetBool("StrongAttack", false);
        m_PlayerAnimator.SetBool("SecondNomalAttck", false);
        m_PlayerAnimator.SetBool("ThirdNomalAttck", false);

    }
}
