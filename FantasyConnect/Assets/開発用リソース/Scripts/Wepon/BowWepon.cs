using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowWepon : MonoBehaviour
{
    enum eAttckType
    {
        NomalAttck,
        StrongAttack,
    }
    private eAttckType attckType;
    [SerializeField]
    private Animator m_PlayerAnimator;
    [SerializeField, Header("攻撃クールタイム")]
    private float m_AttckCoolTime;
    [SerializeField, Header("アニメーションから矢を発射するまでの時間")]
    private float m_ArrowShotTime;
    [SerializeField, Header("通常攻撃用矢")]
    private GameObject m_NomalArrow;
    [SerializeField,Header("強攻撃矢")]
    private GameObject m_StrongArrow;
    [SerializeField, Header("攻撃範囲")]
    private float m_AttckRange=10;
    [SerializeField]
    private Transform bulletSpawnPoint;
    [SerializeField]
    private float bulletForce;
    private bool isAttck = false;
    private bool isWeponChange = true;

    [SerializeField, Header("攻撃ボイス")]
    private List<AudioClip> m_ATVoices; [SerializeField, Header("ボイス")]
    private AudioSource m_Voice;
    void Update()
    {
        ATCoolTime();
        WeponTypeChange();
        if (Input.GetMouseButton(0))
        {
            isWeponChange = false;
            PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
            playerSystem.m_MaxSpeed = 0;

            switch (attckType)
            {
                case eAttckType.NomalAttck:
                    NormalArrowAttack();
                    break;
                case eAttckType.StrongAttack:
                    break;
                default:

                    break;
            }
        }
        else
        {

            isWeponChange = true;
            m_PlayerAnimator.SetBool("StrongAttack", false);
            m_PlayerAnimator.SetBool("NormalAttack", false);
       
        }
    }
    private void NormalArrowAttack()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        playerSystem.m_MaxSpeed = 0;
        StopMoveAnime();
        if (isAttck)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_AttckRange);

            float closestDistance = Mathf.Infinity;
            Transform closestEnemy = null;

            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    m_PlayerAnimator.SetBool("NormalAttack", true);

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

                Vector3 enemyFeetPosition = closestEnemy.position + Vector3.up; // 敵の足元の位置を取得
                Vector3 direction = enemyFeetPosition - bulletSpawnPoint.position; // 弾の発射口から敵の足元への方向を取得

                GameObject bullet = Instantiate(m_NomalArrow, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
                Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

                if (bulletRigidbody != null)
                {
                    bulletRigidbody.AddForce(direction.normalized * bulletForce, ForceMode.Impulse);
                }
                m_AttckCoolTime = 0;

            }
        }
    }
   
    private void ATCoolTime()
    {
        m_AttckCoolTime += Time.deltaTime;
        if (m_AttckCoolTime > 1)
        {
            isAttck = true;
            m_AttckCoolTime = 0;
        }
        else
        {
            isAttck = false;
        }
    }
    private void WeponTypeChange()
    {
        if (isWeponChange)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                attckType = eAttckType.NomalAttck;
            }
            if (Input.GetKey(KeyCode.Alpha2))
            {
                attckType = eAttckType.StrongAttack;
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
            int randomIndex = Random.Range(0, m_ATVoices.Count); // ランダムなインデックスを取得
            AudioClip selectedVoice = m_ATVoices[randomIndex]; // ランダムに選択された攻撃ボイスを取得

            m_Voice.clip = selectedVoice;
            m_Voice.Play();
        }
    }
}
