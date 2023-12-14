using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehindEnemy : MonoBehaviour
{
    public float moveSpeed = 10f; // 移動速度
    public float maxDistance = 10f; // ワープ可能な最大距離
    private Transform targetEnemy; // 最も近い敵のTransform

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            FindNearestEnemy(); // 最も近い敵を探す
            if (targetEnemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);
                if (distanceToEnemy <= maxDistance)
                {
                    MoveBehindEnemy(); // 敵の後ろに移動
                }
                else
                {
                    Debug.Log("敵が範囲外です。ワープできません。");
                }
            }
        }
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            targetEnemy = nearestEnemy.transform;
        }
    }

    void MoveBehindEnemy()
    {
        Vector3 direction = -(targetEnemy.forward); // 敵の後ろに向かう方向

        // 敵の位置から少し離れた位置にプレイヤーを配置
        Vector3 newPosition = targetEnemy.position + (direction * 3f);

        // プレイヤーを新しい位置に瞬時に移動させる
        transform.position = newPosition;
    }
}
