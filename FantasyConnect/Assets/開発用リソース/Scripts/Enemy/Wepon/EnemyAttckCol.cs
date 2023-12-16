using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttckCol : MonoBehaviour
{
    [SerializeField]
    private int Damage=10;
    public bool isAttack=false;
    [SerializeField]
    private GameObject m_HitEffect;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player")&&isAttack)
        {
            if (m_HitEffect != null)
            {
                Instantiate(m_HitEffect, transform.position, Quaternion.identity);
            }
            PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();
            if(playerSystem != null)
            {
                playerSystem.TakeDamage(Damage);
            }
            isAttack = false;
        }
    }
}
