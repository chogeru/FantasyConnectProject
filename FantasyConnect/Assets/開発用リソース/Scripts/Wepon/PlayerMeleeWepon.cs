using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMeleeWepon : MonoBehaviour
{
    [SerializeField, Header("攻撃用コライダー")]
    private GameObject m_AttckCol;
    [SerializeField, Header("パワーアタックコライダー")]
    private GameObject m_PAAttackCol;
    [SerializeField, Header("プレイヤーのアニメーター")]
    private Animator m_PlayerAnimator;
    [SerializeField, Header("通常攻撃トレイル")]
    private GameObject m_AttackTrail;
    [SerializeField,Header("パワーアタックトレイル")]
    private GameObject m_PWAtackTrail;

    [SerializeField, Header("攻撃ボイス")]
    private List<AudioClip> m_ATVoices;
  
    [SerializeField, Header("ボイス")]
    private AudioSource m_Voice;
  
    private void Update()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();

        if (playerSystem.isAttck)
        {
            MelleAttck();
        }
        if (playerSystem.isStrongAttck)
        {
            MelleAttck();
        }
        if (Input.GetMouseButtonUp(0))
        {
            MeleeAttckEnd();
            playerSystem.isAttck = false;
            playerSystem.isStrongAttck = false;
            playerSystem.isWeponChange = true;

        }
    }
    private void MelleAttck()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        AttckSound();
        if(playerSystem.isAttck)
        {
            m_AttackTrail.SetActive(true);
            m_AttckCol.SetActive(true);
        }
        if (playerSystem.isStrongAttck)
        {
            m_PAAttackCol.SetActive(true);
            m_PWAtackTrail.SetActive(true);
        }
    }

    private void MeleeAttckEnd()
    {
        m_PAAttackCol.SetActive(false);
        m_AttckCol.SetActive(false);
        m_AttackTrail.SetActive(false);
        m_PWAtackTrail.SetActive(false);
        EndAttckAnimation();
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        playerSystem.isAttck = false;
        playerSystem.isStrongAttck = false;

    }
    private void EndAttckAnimation()
    {
        m_PlayerAnimator.SetBool("NormalAttack", false);
        m_PlayerAnimator.SetBool("StrongAttack", false);
        m_PlayerAnimator.SetBool("SecondNomalAttck", false);
        m_PlayerAnimator.SetBool("ThirdNomalAttck", false);
    }
    private void AttckSound()
    {
        if (m_ATVoices != null && m_ATVoices.Count > 0)
        {
            int voiceRandomIndex = Random.Range(0, m_ATVoices.Count); // ランダムなインデックスを取得
            AudioClip selectedVoice = m_ATVoices[voiceRandomIndex]; // ランダムに選択された攻撃ボイスを取得

            m_Voice.clip = selectedVoice;
            m_Voice.Play();
        }
    }
}
