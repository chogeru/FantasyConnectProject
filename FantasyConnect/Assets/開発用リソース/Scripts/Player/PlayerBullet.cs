using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField]
    private int Damage = 10;
    [SerializeField, Header("着弾エフェクト")]
    private GameObject m_HitEffect;
    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemySystem enemySystem = collision.gameObject.GetComponent<EnemySystem>();
            if (enemySystem != null)
            {
                enemySystem.TakeDamage(Damage);
            }
        }
        Instantiate(m_HitEffect,transform.position,Quaternion.identity);
        Destroy(gameObject);

    }*/
    private void OnTriggerEnter(Collider other)
    {
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
