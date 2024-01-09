using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehindEnemy : MonoBehaviour
{
    [SerializeField]
    private float m_MoveSpeed = 10f; 
    [SerializeField,Header("���[�v�ő勗��")]
    private float m_MaxDistance = 10f; 
    [SerializeField,Header("���[�v�G�t�F�N�g")]
    private GameObject m_MoveEffect;
    private Transform targetEnemy;
    [SerializeField,Header("����MP")]
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
                        MoveBehindEnemy(); // �G�̌��Ɉړ�
                        playerSystem.m_MP-=m_MPConsumption;
                        playerSystem.MpUpdate();
                    }
                    else
                    {
                        Debug.Log("�G���͈͊O�ł��B���[�v�ł��܂���B");
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
        // �G�̌��Ɍ���������
        Vector3 direction = -(targetEnemy.forward);

        // �G�̈ʒu���班�����ꂽ�ʒu�Ƀv���C���[��z�u//3f
        Vector3 newPosition = targetEnemy.position + (direction * 1.2f);

        // �v���C���[��V�����ʒu�ɏu���Ɉړ�������
        transform.position = newPosition;
        Instantiate(m_MoveEffect, transform.position, Quaternion.identity);
    }
}
