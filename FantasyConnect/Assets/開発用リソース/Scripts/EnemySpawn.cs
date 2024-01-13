using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public List<GameObject> m_EnemyPrefabs; 
    private float m_Timer;
    private float m_SpawnInterval;
    [SerializeField,Header("最大スポーン間隔")]
    private float m_MaxSpawnTime=20;
    [SerializeField,Header("最小スポーン間隔")]
    private float m_MinSpawnTime=10;
    private int m_SpawnCount;

    [SerializeField]
    private AudioSource m_AudioSource;
    [SerializeField,Header("スポーン時の音声")]
    private AudioClip m_SpownClip;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer >= m_SpawnInterval)
        {
            SpawnEnemy();
            ResetTimer();
            m_AudioSource.clip= m_SpownClip;
            m_AudioSource.Play();
        }
    }

    void SpawnEnemy()
    {
        float weight = Mathf.Clamp01((float)m_SpawnCount / 10f);
        int selectedIndex = GetWeightedRandomIndex(weight);
        GameObject selectedPrefab = m_EnemyPrefabs[selectedIndex];
        Instantiate(selectedPrefab, transform.position, Quaternion.identity);

        m_SpawnCount++;
    }

    void ResetTimer()
    {
        m_SpawnInterval = Random.Range(m_MinSpawnTime, m_MaxSpawnTime);
        m_Timer = 0f;
    }

    int GetWeightedRandomIndex(float weight)
    {
        float totalWeight = 0f;

        for (int i = m_EnemyPrefabs.Count - 1; i >= 0; i--)
        {
            totalWeight += Mathf.Lerp(0f, 1f, weight) * (i + 1);
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = m_EnemyPrefabs.Count - 1; i >= 0; i--)
        {
            cumulativeWeight += Mathf.Lerp(0f, 1f, weight) * (i + 1);
            if (randomValue <= cumulativeWeight)
            {
                return i;
            }
        }
        return 0;
    }
}
