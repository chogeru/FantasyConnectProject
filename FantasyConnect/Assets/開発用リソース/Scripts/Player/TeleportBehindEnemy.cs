using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehindEnemy : MonoBehaviour
{
    [SerializeField]
    private float m_MoveSpeed = 10f; 
    [SerializeField,Header("ワープ最大距離")]
    private float m_MaxDistance = 10f; 
    [SerializeField,Header("ワープエフェクト")]
    private GameObject m_MoveEffect;
    private Transform targetEnemy;
    [SerializeField,Header("消費MP")]
    private int m_MPConsumption;
    [SerializeField]
    AnimalRideSystem animalRideSystem;
    [SerializeField]
    PlayerSystem playerSystem;
    void Update()
    {
        if (!animalRideSystem.isRide)
        {
            if (Input.GetKeyDown(KeyCode.Q)&&playerSystem.m_MP>=m_MPConsumption)
            {
                FindNearestEnemy();
                if (targetEnemy != null)
                {
                    float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);
                    if (distanceToEnemy <= m_MaxDistance)
                    {
                        Instantiate(m_MoveEffect, transform.position, Quaternion.identity);
                        MoveBehindEnemy(); // 敵の後ろに移動
                        playerSystem.m_MP-=m_MPConsumption;
                        playerSystem.MpUpdate();
                    }
                    else
                    {
                        Debug.Log("敵が範囲外です。ワープできません。");
                    }
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
        // 敵の後ろに向かう方向
        Vector3 direction = -(targetEnemy.forward);

        // 敵の位置から少し離れた位置にプレイヤーを配置//3f
        Vector3 newPosition = targetEnemy.position + (direction * 1.2f);

        // プレイヤーを新しい位置に瞬時に移動させる
        transform.position = newPosition;
        Instantiate(m_MoveEffect, transform.position, Quaternion.identity);
    }
}
