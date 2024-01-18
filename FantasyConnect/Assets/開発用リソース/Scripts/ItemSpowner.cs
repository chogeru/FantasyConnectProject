using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpowner : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> m_ItemPrefab;
    private List<GameObject> spawnedItems = new List<GameObject>();

    [SerializeField]
    private int m_ItemSpawnNunber = 3;
    [SerializeField]
    private float m_SpqwnRadius = 5f;
    [SerializeField]
    private float m_SpawnHeight = 2f;
    [SerializeField]
    private float m_Distance = 5f;
    [SerializeField]
    private float m_DestroyTime;
    [SerializeField]
    private GameObject m_SpawnEffect;
    [SerializeField]
    private GameObject m_SpawnEffect2;
    [SerializeField]
    private AudioSource m_SpawnSE;
    [SerializeField, Header("アニメーター")]
    private Animator m_Animator;
    private bool isSpawn = false;
    private bool isDestroy = false;
    private bool isEnd = false;
    void Update()
    {
        if (isEnd) return;
        if (isSpawn)
        {
            Invoke("StopAllProcesses", 3f);
            for (int i = 0; i < spawnedItems.Count; i++)
            {
                Vector3 newPosition = spawnedItems[i].transform.position;
                newPosition.y -= 0.11f * Time.deltaTime;
                spawnedItems[i].transform.position = newPosition;
            }
            return;
        }
        if (IsPlayerOrBotInRange())
        {
            m_Animator.SetBool("Open", true);
        }
    }
    private bool IsPlayerOrBotInRange()
    {
        RaycastHit hit;
        Vector3 rayDirection = transform.forward;
        rayDirection.y = 0f;
        Debug.DrawRay(transform.position, rayDirection.normalized * m_Distance, Color.red);

        if (Physics.Raycast(transform.position, transform.forward, out hit, m_Distance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
    public void SpawnItems()
    {
        for (int i = 0; i < m_ItemSpawnNunber; i++)
        {
            // ランダムな角度で生成位置を計算
            float angle = Random.Range(0f, 360f);
            Vector3 spawnPosition = transform.position + Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * m_SpqwnRadius);

            // アイテムの高さを追加
            spawnPosition.y = transform.position.y + m_SpawnHeight;
            Instantiate(m_SpawnEffect, transform.position, Quaternion.identity);
            Instantiate(m_SpawnEffect2, transform.position + Vector3.down * 0.48f, Quaternion.identity);

            // ランダムにアイテムを選んで生成
            GameObject selectedPrefab = m_ItemPrefab[Random.Range(0, m_ItemPrefab.Count)];
            GameObject spawnedItem = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            spawnedItems.Add(spawnedItem);
        }
        isSpawn = true;
        isDestroy = true;
        m_SpawnSE.Play();
    }
    private void StopAllProcesses()
    {
        isEnd = true;
    }
}
