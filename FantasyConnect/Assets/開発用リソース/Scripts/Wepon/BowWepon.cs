using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowWepon : MonoBehaviour
{
    [SerializeField]
    private Animator m_PlayerAnimator;
    [SerializeField, Header("�U���N�[���^�C��")]
    private float m_AttckCoolTime;
    [SerializeField, Header("�A�j���[�V���������𔭎˂���܂ł̎���")]
    private float m_ArrowShotTime;
    [SerializeField, Header("�ʏ�U���p��")]
    private GameObject m_NomalArrow;
    [SerializeField, Header("���U����")]
    private GameObject m_StrongArrow;
    [SerializeField, Header("�U���͈�")]
    private float m_AttckRange = 10;
    [SerializeField]
    private Transform m_NomalBulletSpawnPoint;
    [SerializeField]
    private Transform m_StrongBulletSpawnPoint;
    [SerializeField]
    private float bulletForce;
    private bool isAttck = false;


    [SerializeField, Header("�U���{�C�X")]
    private List<AudioClip> m_ATVoices; [SerializeField, Header("�{�C�X")]
    private AudioSource m_Voice;
    [SerializeField]
    private AudioSource m_ShotSE;
    void Update()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        if (playerSystem.isAttck)
        {
            ArrowAttack();
        }
        if (playerSystem.isStrongAttck)
        {
            StrongAttack();
        }
        if (Input.GetMouseButtonUp(0))
        {
            playerSystem.isWeponChange = true;
            m_PlayerAnimator.SetBool("StrongAttack", false);
            m_PlayerAnimator.SetBool("NormalAttack", false);
        }
    }
    void ArrowAttack()
    {
        m_ShotSE.Play();
        AttckSound();
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        playerSystem.isAttck = false;
        playerSystem.isStrongAttck = false;
        StopMoveAnime();
        Vector3 direction = m_NomalBulletSpawnPoint.forward; // �e���̌������擾����

        // ��̌������e���̌����ɍ��킹��
        Quaternion rotation = Quaternion.LookRotation(direction);

        // ��U�A��������e���̏�����ɐݒ�
        Vector3 correctedUpDirection = m_NomalBulletSpawnPoint.up;

        // ��̉�]�𒲐����ĉ������ɂ���
        rotation = Quaternion.LookRotation(direction, correctedUpDirection);

        GameObject bullet = Instantiate(m_NomalArrow, m_NomalBulletSpawnPoint.position, rotation);
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

        if (bulletRigidbody != null)
        {
            bulletRigidbody.AddForce(direction * bulletForce, ForceMode.Impulse);
        }

    }

    void StrongAttack()
    {
        m_ShotSE.Play();
        AttckSound();
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        playerSystem.isAttck = false;
        playerSystem.isStrongAttck = false;
        StopMoveAnime();

        Vector3 direction = m_StrongBulletSpawnPoint.forward; // �O�������擾

        for (int i = 0; i < 3; i++)
        {
            Quaternion rotation = Quaternion.LookRotation(direction); // �O�����ւ̉�]���擾

            GameObject bullet = Instantiate(m_StrongArrow, m_StrongBulletSpawnPoint.position, rotation);
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

            if (bulletRigidbody != null)
            {
                // �����ɔ��˂��邽�߂̕����C���i��𐅕��ɍL����j
                Vector3 horizontalDirection = Quaternion.AngleAxis(-15f + (i * 15f), Vector3.up) * direction;
                bulletRigidbody.AddForce(horizontalDirection * bulletForce, ForceMode.Impulse);
            }
        }
    }

        private void StopMoveAnime()
    {
        m_PlayerAnimator.SetBool("Idle", false);
        m_PlayerAnimator.SetBool("Walk", false);
        m_PlayerAnimator.SetBool("Run", false);
    }
    private void AttckSound()
    {
        if (m_ATVoices != null && m_ATVoices.Count > 0)
        {
            int randomIndex = Random.Range(0, m_ATVoices.Count); // �����_���ȃC���f�b�N�X���擾
            AudioClip selectedVoice = m_ATVoices[randomIndex]; // �����_���ɑI�����ꂽ�U���{�C�X���擾

            m_Voice.clip = selectedVoice;
            m_Voice.Play();
        }
    }
}
