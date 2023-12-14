using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeMagicAttckCol : MonoBehaviour
{
    [SerializeField]
    private int Damage = 10;
    private float m_CoolTime;
    private bool isAttck=false;
    private List<EnemySystem> enemysInRange=new List<EnemySystem>();
    private void Update()
    {
        m_CoolTime += Time.deltaTime;
        if(m_CoolTime>1)
        {
            isAttck = true;
        }
    }
    private void FixedUpdate()
    {
        if (isAttck && enemysInRange.Count > 0)
        {
            isAttck = false;
            m_CoolTime = 0;
            enemysInRange.Clear();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(isAttck) 
        {
            if(other.gameObject.CompareTag("Enemy"))
            {
                EnemySystem enemySystem = other.GetComponent<EnemySystem>();
                if (enemySystem != null && !enemysInRange.Contains(enemySystem))
                {
                    enemysInRange.Add(enemySystem);
                    enemySystem.TakeDamage(Damage);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemySystem enemySystem = other.GetComponent<EnemySystem>();
            if (enemySystem != null && enemysInRange.Contains(enemySystem))
            {
                enemysInRange.Remove(enemySystem);
            }
        }
    }
}
