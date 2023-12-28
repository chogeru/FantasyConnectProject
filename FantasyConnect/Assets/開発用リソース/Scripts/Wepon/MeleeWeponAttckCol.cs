using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeponAttckCol : MonoBehaviour
{
    [SerializeField]
    private int Damage = 80;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemySystem enemySystem = other.GetComponent<EnemySystem>();

            enemySystem.TakeDamage(Damage);

        }
    }

}
