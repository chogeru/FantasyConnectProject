using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttckCol : MonoBehaviour
{
    [SerializeField]
    private int Damage=10;
    public bool isAttack=false;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player")&&isAttack)
        {
            PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();
            if(playerSystem != null)
            {
                playerSystem.TakeDamage(Damage);
            }
            isAttack = false;
        }
    }
}
