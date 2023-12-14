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
    [SerializeField, Header("UŒ‚ƒN[ƒ‹ƒ^ƒCƒ€")]
    private float m_AttackCoolTime;
    [SerializeField]
    private Animator m_PlayerAnimator;
    private bool isAttck=false;
    [SerializeField]
    private GameObject m_MagicBullet;
    [SerializeField]
    private GameObject m_RangeMagic;
    [SerializeField]
    private GameObject m_RangeAttckCol;
    [SerializeField,Header("UŒ‚”ÍˆÍ")]
    private float m_AttckRange = 10f;
    [SerializeField]
    private Transform bulletSpawnPoint;
    [SerializeField]
    private float bulletForce;

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
                Vector3 direction = closestEnemy.position - bulletSpawnPoint.position; // ’e‚Ì”­ŽËŒû‚©‚ç‚Ì•ûŒü‚ðŽæ“¾
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


    private void StopMoveAnime()
    {
        m_PlayerAnimator.SetBool("Idle", false);
        m_PlayerAnimator.SetBool("Walk", false);
        m_PlayerAnimator.SetBool("Run", false);
    }
}
