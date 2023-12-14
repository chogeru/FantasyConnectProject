using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeMagicAttckCol : MonoBehaviour
{
    [SerializeField]
    private int Damage = 10;
    private float m_CoolTime;
    private bool isAttck=false;
    private void Update()
    {
        m_CoolTime += Time.deltaTime;
        if(m_CoolTime>1)
        {
            isAttck = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(isAttck) 
        {
            if(other.gameObject.CompareTag("Enemy"))
            {
                EnemySystem enemySystem =other.GetComponent<EnemySystem>();
                if(enemySystem != null)
                {
                    enemySystem.TakeDamage(Damage);
                    isAttck=false;
                    m_CoolTime = 0;
                }
            }
        }
    }
}
