using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BossEnemyManager : MonoBehaviour
{
    [SerializeField,Header("�Έڂ���V�[����")]
    private string m_SceneName;
    [SerializeField,Header("�V�[����̓G�̃��X�g")]
    private List<GameObject> m_BossEnemys = new List<GameObject>();

    [SerializeField,Header("�G���j��^�C�����C��")]
    private GameObject m_BossDestroyTimeline;
    void Start()
    {
        GameObject[] bossEnemyObj = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject boss in bossEnemyObj)
        {
            m_BossEnemys.Add(boss);
        }
    }

  public void DestroyBossEnemy(GameObject boss)
    {
        if(m_BossEnemys.Contains(boss))
        {
            m_BossEnemys.Remove(boss);
            if(m_BossEnemys.Count==0)
            {
                m_BossDestroyTimeline.SetActive(true);
            }
        }
    }
    public void TimelineSceneChange()
    {
        SceneManager.LoadScene(m_SceneName, LoadSceneMode.Single);
    }
}
