using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeponUI : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    [SerializeField, Header("弓武器のUI")]
    private GameObject m_BowWeponUI;
    [SerializeField, Header("近接武器のUI")]
    private GameObject m_MeleeWeponUI;
    [SerializeField, Header("魔法武器のUI")]
    private GameObject m_MagicWeponUI;
    [SerializeField, Header("魔法武器のスライダーUI")]
    private GameObject m_MagicTypeSliderUI;
    [SerializeField, Header("矢の本数表示テキスト")]
    private GameObject m_ArrowText;
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
                m_MagicTypeSliderUI.SetActive(true);
                m_ArrowText.SetActive(false);
                break;
            case PlayerSystem.PlayerType.Bow:
                animator.SetBool("BowActive", true);
                animator.SetBool("MagicActive", false);
                animator.SetBool("MeleeActive", false);
                m_BowWeponUI.SetActive(true);
                m_MagicWeponUI.SetActive(false);
                m_MeleeWeponUI.SetActive(false);
                m_MagicTypeSliderUI.SetActive(false); 
                m_ArrowText.SetActive(true);
                break;
            case PlayerSystem.PlayerType.Melee:
                animator.SetBool("MeleeActive", true);
                animator.SetBool("MagicActive", false);
                animator.SetBool("BowActive", false);
                m_BowWeponUI.SetActive(false);
                m_MagicWeponUI.SetActive(false);
                m_MeleeWeponUI.SetActive(true);
                m_MagicTypeSliderUI.SetActive(false);
                m_ArrowText.SetActive(false);



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
