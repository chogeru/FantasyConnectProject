using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BossAreaStartSetting : MonoBehaviour
{
    private GameObject player;
    [SerializeField]
    private GameObject m_TimelineObj;
    [SerializeField]
    private GameObject m_AnimationEnemy;
    [SerializeField]
    private GameObject m_BossCountUI;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        m_TimelineObj = this.gameObject;
    }
    public void StartBossAnime()
    {
        player.SetActive(false);
        m_AnimationEnemy.SetActive(true);
        m_BossCountUI.SetActive(false);
    }

    public void EndBossAnime()
    {
        player.SetActive(true);
        m_TimelineObj.SetActive(false);
        m_AnimationEnemy.SetActive(false);
        m_BossCountUI.SetActive(true);
    }
}
