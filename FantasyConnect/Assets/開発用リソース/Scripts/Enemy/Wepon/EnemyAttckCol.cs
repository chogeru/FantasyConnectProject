using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttckCol : MonoBehaviour
{
    [SerializeField]
    private int Damage=10;
    public bool isAttack=false;

    EnemyATEffectObjctPool m_EnemyATEffectObjctPool;
    private void Start()
    {
        m_EnemyATEffectObjctPool = EnemyATEffectObjctPool.Instance;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player")&&isAttack)
        {
            if (m_EnemyATEffectObjctPool != null)
            {
                GameObject hitEffect = m_EnemyATEffectObjctPool.GetPooledObject();
                hitEffect.transform.position = transform.position;
                hitEffect.SetActive(true);
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
