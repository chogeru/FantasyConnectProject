using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField]
    private int Damage = 10;
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
        Destroy(gameObject);

    }
}
