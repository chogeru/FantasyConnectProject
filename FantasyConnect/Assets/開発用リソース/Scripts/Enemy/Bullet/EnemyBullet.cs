using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField,Header("ヒットした時のエフェクト")]
    private GameObject m_HitEffect;
    [SerializeField]
    private int Damage=10;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerSystem playerSystem = collision.gameObject.GetComponent<PlayerSystem>();
            if(playerSystem != null)
            {
                playerSystem.TakeDamage(Damage);
            }
        }
        Instantiate(m_HitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
