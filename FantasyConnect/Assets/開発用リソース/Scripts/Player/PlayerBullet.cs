using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField]
    private int Damage = 10;
    [SerializeField, Header("着弾エフェクト")]
    private GameObject m_HitEffect;

    // プレイヤーレイヤーのマスク
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
        // プレイヤーレイヤーに属するオブジェクトを無視する
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
        // プレイヤーレイヤーに属するオブジェクトを無視する
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
