using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMeleeWepon : MonoBehaviour
{
    [SerializeField, Header("�U���p�R���C�_�[")]
    private GameObject m_AttckCol;
    [SerializeField, Header("�p���[�A�^�b�N�R���C�_�[")]
    private GameObject m_PAAttackCol;
    [SerializeField, Header("�v���C���[�̃A�j���[�^�[")]
    private Animator m_PlayerAnimator;
    [SerializeField, Header("�ʏ�U���g���C��")]
    private GameObject m_AttackTrail;
    [SerializeField,Header("�p���[�A�^�b�N�g���C��")]
    private GameObject m_PWAtackTrail;

    [SerializeField, Header("�U���{�C�X")]
    private List<AudioClip> m_ATVoices;
  
    [SerializeField, Header("�{�C�X")]
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
            int voiceRandomIndex = Random.Range(0, m_ATVoices.Count); // �����_���ȃC���f�b�N�X���擾
            AudioClip selectedVoice = m_ATVoices[voiceRandomIndex]; // �����_���ɑI�����ꂽ�U���{�C�X���擾

            m_Voice.clip = selectedVoice;
            m_Voice.Play();
        }
    }
}
