using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrackingBullet : MonoBehaviour
{
    [SerializeField,Header("タグ")]
    private string m_PlayerTag = "Player";
    private Transform playerTransform;
    [SerializeField,Header("追跡する時間")]
    private float m_FollowTime = 5f;
    [SerializeField,Header("位置の保管の値")]
    private float m_SmoothDamping = 5f;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(m_PlayerTag);

        if (player != null)
        {
            playerTransform = player.transform;
            // 一定時間後に追跡停止するコルーチンを開始
            StartCoroutine(StopFollowingAfterTime());
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            //若干位置を上に調整
            Vector3 targetPosition = playerTransform.position + Vector3.up * 1f;
            targetPosition.y = transform.position.y;
            // 目標位置までスムーズに移動させる
            transform.position = Vector3.Lerp(transform.position, targetPosition, m_SmoothDamping * Time.deltaTime);
        }
    }

    IEnumerator StopFollowingAfterTime()
    {
        yield return new WaitForSeconds(m_FollowTime);
        enabled = false;
    }
}
