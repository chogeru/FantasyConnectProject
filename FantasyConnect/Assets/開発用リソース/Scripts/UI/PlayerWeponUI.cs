using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeponUI : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    [SerializeField, Header("ã|ïêäÌÇÃUI")]
    private GameObject m_BowWeponUI;
    [SerializeField, Header("ãﬂê⁄ïêäÌÇÃUI")]
    private GameObject m_MeleeWeponUI;
    [SerializeField, Header("ñÇñ@ïêäÌÇÃUI")]
    private GameObject m_MagicWeponUI;
    [SerializeField]
    private RectTransform m_ArrowImage;

    [SerializeField]
    PlayerSystem playerSystem;

    private void Start()
    {
        WeponUIUpdate();
    }
    private void Update()
    {
        WeponUIUpdate();  
    }
    private void WeponUIUpdate()
    {
        switch (playerSystem.playerType)
        {
            case PlayerSystem.PlayerType.Magic:
                animator.SetBool("MagicActive", true);
                animator.SetBool("BowActive", false);
                animator.SetBool("MeleeActive", false);
                m_MeleeWeponUI.SetActive(false);
                m_BowWeponUI.SetActive(false);
                m_MagicWeponUI.SetActive(true);
                break;
            case PlayerSystem.PlayerType.Bow:
                animator.SetBool("BowActive", true);
                animator.SetBool("MagicActive", false);
                animator.SetBool("MeleeActive", false);
                m_BowWeponUI.SetActive(true);
                m_MagicWeponUI.SetActive(false);
                m_MeleeWeponUI.SetActive(false);
                break;
            case PlayerSystem.PlayerType.Melee:
                animator.SetBool("MeleeActive", true);
                animator.SetBool("MagicActive", false);
                animator.SetBool("BowActive", false);
                m_BowWeponUI.SetActive(false);
                m_MagicWeponUI.SetActive(false);
                m_MeleeWeponUI.SetActive(true);


                break;
            default:
                break;
        }
        switch (playerSystem.attckType)
        {
            case PlayerSystem.eAttckType.NomalAttck:
                m_ArrowImage.anchoredPosition = new Vector2(-300, 200);
                break;
            case PlayerSystem.eAttckType.StrongAttack:
                m_ArrowImage.anchoredPosition = new Vector2(-100, 200);
                break;
            default:
                break;
        }
    }
}
