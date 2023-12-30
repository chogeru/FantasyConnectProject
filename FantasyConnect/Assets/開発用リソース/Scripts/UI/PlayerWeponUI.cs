using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeponUI : MonoBehaviour
{
    [SerializeField, Header("�|�����UI")]
    private GameObject m_BowWeponUI;
    [SerializeField, Header("�ߐڕ����UI")]
    private GameObject m_MeleeWeponUI;
    [SerializeField, Header("���@�����UI")]
    private GameObject m_MagicWeponUI;
    [SerializeField]
    PlayerSystem playerSystem;

    private void Update()
    {
        switch (playerSystem.playerType)
        {
            case PlayerSystem.PlayerType.Magic:
                m_BowWeponUI.SetActive(false);
                m_MagicWeponUI.SetActive(true);
                m_MeleeWeponUI.SetActive(false);
                break;
            case PlayerSystem.PlayerType.Bow:
                m_BowWeponUI.SetActive(true);
                m_MagicWeponUI.SetActive(false);
                m_MeleeWeponUI.SetActive(false);
                break;
                case PlayerSystem.PlayerType.Melee:
                m_BowWeponUI.SetActive(false);
                m_MagicWeponUI.SetActive(false);
                m_MeleeWeponUI.SetActive(true);
                break;
            default:
                break;
        }

    }
}
