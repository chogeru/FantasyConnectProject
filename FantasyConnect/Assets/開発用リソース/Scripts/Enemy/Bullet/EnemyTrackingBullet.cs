using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrackingBullet : MonoBehaviour
{
    [SerializeField,Header("�^�O")]
    private string m_PlayerTag = "Player";
    private Transform playerTransform;
    [SerializeField,Header("�ǐՂ��鎞��")]
    private float m_FollowTime = 5f;
    [SerializeField,Header("�ʒu�̕ۊǂ̒l")]
    private float m_SmoothDamping = 5f;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(m_PlayerTag);

        if (player != null)
        {
            playerTransform = player.transform;
            // ��莞�Ԍ�ɒǐՒ�~����R���[�`�����J�n
            StartCoroutine(StopFollowingAfterTime());
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            //�኱�ʒu����ɒ���
            Vector3 targetPosition = playerTransform.position + Vector3.up * 1f;
            targetPosition.y = transform.position.y;
            // �ڕW�ʒu�܂ŃX���[�Y�Ɉړ�������
            transform.position = Vector3.Lerp(transform.position, targetPosition, m_SmoothDamping * Time.deltaTime);
        }
    }

    IEnumerator StopFollowingAfterTime()
    {
        yield return new WaitForSeconds(m_FollowTime);
        enabled = false;
    }
}
