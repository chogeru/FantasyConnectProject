using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField]
    private int Damage = 10;
    [SerializeField, Header("���e�G�t�F�N�g")]
    private GameObject m_HitEffect;

    // �v���C���[���C���[�̃}�X�N
    public LayerMask playerLayerMask;
    private GameObject m_ShotSE;
    private void Start()
    {
        GameObject hitSE =  PlayerBulletSEPool.Instance.GetPooledObject();
        hitSE.transform.position = transform.position;
        hitSE.SetActive(true);
    }
    private void OnCollisionEnter(Collision collision)
    {
        // �v���C���[���C���[�ɑ�����I�u�W�F�N�g�𖳎�����
        if ((playerLayerMask.value & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemySystem enemySystem = collision.gameObject.GetComponent<EnemySystem>();
            if (enemySystem != null)
            {
                enemySystem.TakeDamage(Damage);
            }
        }
        Instantiate(m_HitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        // �v���C���[���C���[�ɑ�����I�u�W�F�N�g�𖳎�����
        if ((playerLayerMask.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
        {
            return;
        }

        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemySystem enemySystem = other.gameObject.GetComponent<EnemySystem>();
            if (enemySystem != null)
            {
                enemySystem.TakeDamage(Damage);
            }
        }
        Instantiate(m_HitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
