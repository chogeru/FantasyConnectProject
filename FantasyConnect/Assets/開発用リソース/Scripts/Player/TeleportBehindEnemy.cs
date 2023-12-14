using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehindEnemy : MonoBehaviour
{
    public float moveSpeed = 10f; // �ړ����x
    public float maxDistance = 10f; // ���[�v�\�ȍő勗��
    private Transform targetEnemy; // �ł��߂��G��Transform

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            FindNearestEnemy(); // �ł��߂��G��T��
            if (targetEnemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);
                if (distanceToEnemy <= maxDistance)
                {
                    MoveBehindEnemy(); // �G�̌��Ɉړ�
                }
                else
                {
                    Debug.Log("�G���͈͊O�ł��B���[�v�ł��܂���B");
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
        Vector3 direction = -(targetEnemy.forward); // �G�̌��Ɍ���������

        // �G�̈ʒu���班�����ꂽ�ʒu�Ƀv���C���[��z�u
        Vector3 newPosition = targetEnemy.position + (direction * 3f);

        // �v���C���[��V�����ʒu�ɏu���Ɉړ�������
        transform.position = newPosition;
    }
}
