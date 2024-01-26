using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class BossEnemyManager : MonoBehaviour
{
    [SerializeField, Header("偏移するシーン名")]
    private string m_SceneName;
    [SerializeField, Header("シーン上の敵のリスト")]
    private List<GameObject> m_BossEnemys = new List<GameObject>();

    [SerializeField, Header("敵撃破後タイムライン")]
    private GameObject m_BossDestroyTimeline;

    [SerializeField]
    private GameObject m_ResultTimeline;
    [SerializeField, Header("残り敵の数表示UI")]
    private TextMeshProUGUI m_EnemyCountText;
    [SerializeField]
    private TextMeshProUGUI m_TimeCountText;
    private float m_Time;
    private bool isCount = true;
    private int m_EnemyCount;
    void Start()
    {
        GameObject[] bossEnemyObj = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject boss in bossEnemyObj)
        {
            m_BossEnemys.Add(boss);
        }
        m_EnemyCount = m_BossEnemys.Count;
        UpdateEnemyCountUI();
    }

    private void Update()
    {
        if (isCount)
        {
            m_Time += Time.deltaTime;
            if (m_TimeCountText != null)
            {
                string formattedTime = string.Format("{0:00}:{1:00}", Mathf.Floor(m_Time / 60), Mathf.Floor(m_Time % 60));
                m_TimeCountText.text = "タイム: " + formattedTime;
            }
        }

    }

    public void DestroyBossEnemy(GameObject boss)
    {
        if (boss != null && m_BossEnemys.Contains(boss))
        {
            m_BossEnemys.Remove(boss);
            UpdateEnemyCountUI();
            if (m_BossEnemys.Count == 0)
            {
                isCount = false;
                m_BossDestroyTimeline.SetActive(true);
            }
        }
    }
    public void SetResultTimeline()
    {
        m_ResultTimeline.SetActive(true);
        Cursor.visible = true;
    }
    public void LoadCity()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen(m_SceneName);
    }
    private void UpdateEnemyCountUI()
    {
        int remainingEnemies = m_BossEnemys.Count;
        if (m_EnemyCountText != null)
        {
            m_EnemyCountText.text = "残り敵の数: " + remainingEnemies + " / " + m_EnemyCount;
        }
    }
}
