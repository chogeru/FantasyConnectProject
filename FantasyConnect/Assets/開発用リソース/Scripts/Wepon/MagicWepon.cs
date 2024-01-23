using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MagicWepon : MonoBehaviour
{
    [SerializeField]
    PlayerSystem playerSystem;
    [SerializeField, Header("攻撃クールタイム")]
    private float m_AttackCoolTime;
    [SerializeField]
    private Animator m_PlayerAnimator;
    [SerializeField, Header("魔法弾攻撃")]
    private GameObject m_MagicBullet;
    [SerializeField, Header("範囲攻撃魔法")]
    private GameObject m_RangeMagic;
    [SerializeField, Header("範囲攻撃用コライダー")]
    private GameObject m_RangeAttckCol;
    [SerializeField, Header("魔法攻撃時のエフェクト")]
    private GameObject m_AtckEffect;
    [SerializeField, Header("攻撃範囲")]
    private float m_AttckRange = 10f;
    [SerializeField]
    private Transform bulletSpawnPoint;
    [SerializeField]
    private float bulletForce;

    private bool isRangeAttck=false;

    private bool isCoolDown=false;
    private float m_ParticlCoolTime;
    [SerializeField, Header("攻撃ボイス")]
    private List<AudioClip> m_ATVoices; [SerializeField, Header("ボイス")]
    private AudioSource m_Voice;
    void Update()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        if (playerSystem.isAttck)
        {
            MagicBulletAttack();
        }
        if (playerSystem.isStrongAttck)
        {
            MagicRangeAttack();
        }
        if(Input.GetMouseButtonUp(0)||Input.GetMouseButtonUp(1))
        {

            playerSystem.isWeponChange = true;
            m_PlayerAnimator.SetBool("StrongAttack", false);
            m_PlayerAnimator.SetBool("NormalAttack", false);
            m_RangeAttckCol.SetActive(false);
            m_AtckEffect.SetActive(false);
        }
        if (!m_PlayerAnimator.GetBool("StrongAttack") && !m_PlayerAnimator.GetBool("NormalAttack"))
        {
            m_RangeAttckCol.SetActive(false);
            m_AtckEffect.SetActive(false);
        }
        if (isRangeAttck)
        {
            Invoke("EndMagicRangeAttck", 1.5f);
        }
        if (!isCoolDown)
        {
            m_ParticlCoolTime += Time.deltaTime;
        }
        if(m_ParticlCoolTime>0.5)
        {
            isCoolDown = true;
            m_ParticlCoolTime = 0;
        }
    }
    private void MagicBulletAttack()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        playerSystem.isAttck = false;
        playerSystem.isStrongAttck = false;
        StopMoveAnime();
        m_AtckEffect.SetActive(true);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_AttckRange);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        if (playerSystem.m_MP >= 5)
        {
            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    AttckSound();
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = collider.transform;
                    }
                }
            }

            if (closestEnemy != null)
            {
                Vector3 direction = closestEnemy.position + Vector3.up * 1f - bulletSpawnPoint.position; // 弾の発射口からの方向を取得
                GameObject bullet = Instantiate(m_MagicBullet, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
                Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

                if (bulletRigidbody != null)
                {
                    bulletRigidbody.AddForce(direction.normalized * bulletForce, ForceMode.Impulse);
                }
                playerSystem.m_MP -= 5;
                playerSystem.MpUpdate();
            }
        }
    }
    private void MagicRangeAttack()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        if (playerSystem.m_MP >= 10)
        {
            StopMoveAnime();
            isRangeAttck = true;
            AttckSound();
            m_RangeAttckCol.SetActive(true);
            if (isCoolDown)
            {
                Instantiate(m_RangeMagic, transform.position, Quaternion.identity);
                isCoolDown = false;
            }
        }
    }

    public void AttackEnd()
    {
        m_RangeAttckCol.SetActive(false);
        m_AtckEffect.SetActive(false);
    }
    private void EndMagicRangeAttck()
    {
        playerSystem.isAttck = false;
        playerSystem.isStrongAttck = false;
        isRangeAttck = false;
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
            int randomIndex = Random.Range(0, m_ATVoices.Count); // ランダムなインデックスを取得
            AudioClip selectedVoice = m_ATVoices[randomIndex]; // ランダムに選択された攻撃ボイスを取得

            m_Voice.clip = selectedVoice;
            m_Voice.Play();
        }
    }
}
