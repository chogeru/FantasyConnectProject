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
    private bool isWeponChange = true;
    [SerializeField,Header("魔法弾攻撃")]
    private GameObject m_MagicBullet;
    [SerializeField,Header("範囲攻撃魔法")]
    private GameObject m_RangeMagic;
    [SerializeField,Header("範囲攻撃用コライダー")]
    private GameObject m_RangeAttckCol;
    [SerializeField,Header("魔法攻撃時のエフェクト")]
    private GameObject m_AtckEffect;
    [SerializeField,Header("攻撃範囲")]
    private float m_AttckRange = 10f;
    [SerializeField]
    private Transform bulletSpawnPoint;
    [SerializeField]
    private float bulletForce;

    void Update()
    {
        ATCoolTime();
      WeponTypeChange();
        if(Input.GetMouseButton(0))
        {
            isWeponChange = false;
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

            isWeponChange=true;
            m_PlayerAnimator.SetBool("MagicAttck", false);
            m_PlayerAnimator.SetBool("MagicBulletAttck", false);
            m_RangeAttckCol.SetActive(false);
            m_AtckEffect.SetActive(false);
        }
    }
    private void MagicBulletAttack()
    {
        PlayerSystem playerSystem =GetComponentInParent<PlayerSystem>();
        playerSystem.m_MaxSpeed = 0; 
        StopMoveAnime();
        m_PlayerAnimator.SetBool("MagicBulletAttck", true);
        m_AtckEffect.SetActive(true);
        if (isAttck)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_AttckRange);

            float closestDistance = Mathf.Infinity;
            Transform closestEnemy = null;

            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy"))
                {
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
                Vector3 direction = closestEnemy.position+Vector3.up*1f - bulletSpawnPoint.position; // 弾の発射口からの方向を取得
                GameObject bullet = Instantiate(m_MagicBullet, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
                Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

                if (bulletRigidbody != null)
                {
                    bulletRigidbody.AddForce(direction.normalized * bulletForce, ForceMode.Impulse);
                }
                m_AttackCoolTime = 0;
            }
        }
    }
    private void MagicRangeAttack()
    {
        StopMoveAnime();
        m_PlayerAnimator.SetBool("MagicAttck", true);
        if (isAttck)
        {
            m_RangeAttckCol.SetActive(true);
            Instantiate(m_RangeMagic, transform.position, Quaternion.identity);
            m_AttackCoolTime = 0;

        }
    }
    private void ATCoolTime()
    {
        m_AttackCoolTime += Time.deltaTime;
        if (m_AttackCoolTime > 1)
        {
            isAttck = true;
            m_AttackCoolTime = 0;
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
                attckType = eAttckType.Bullet;
            }
            if (Input.GetKey(KeyCode.Alpha2))
            {
                attckType = eAttckType.RangeAttack;
            }
        }
    }
    private void StopMoveAnime()
    {
        m_PlayerAnimator.SetBool("Idle", false);
        m_PlayerAnimator.SetBool("Walk", false);
        m_PlayerAnimator.SetBool("Run", false);
    }
}
