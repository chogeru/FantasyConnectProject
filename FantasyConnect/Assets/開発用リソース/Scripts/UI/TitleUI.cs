using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    // UI��Canvas�I�u�W�F�N�g�ւ̎Q��
    [SerializeField,Header("�J�n���̃Z���N�g�{�^��")]
    private GameObject m_TitleCanvas;
    [SerializeField,Header("�J�n����Text�pCanvas")]
    private GameObject m_TextCanvas;
    [SerializeField]
    private GameObject m_Fade;
    [SerializeField]
    CurrencySystem m_CurrencySystem;
    [SerializeField]
    InventorySystem m_InventorySystem;
    [SerializeField]
    SceneSave m_SceneData;
    // �t�F�[�h�C���̑��x�����p�p�����[�^
    [SerializeField]
    private float m_EaseSpeed = 0.01f;

    // ���݂̃A���t�@�l��ێ�����ϐ�
    private float m_CurrentAlpha = 0.0f;

    // �t���[�����ƂɌĂяo�����Unity��Update���\�b�h
    private void Update()
    {
        // �C�ӂ̃L�[�������ꂽ�ꍇ�A�t�F�[�h�C���������J�n����
        if (Input.anyKeyDown)
        {
            m_TextCanvas.SetActive(false);
            m_TitleCanvas.SetActive(true);
            StartCoroutine(FadeInCanvas());
        }
    }
    public void ResetStart()
    {
        m_CurrencySystem.ResetCurrency();
        m_InventorySystem.ResetItem();
        m_SceneData.ResetSceneData();
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("TutorialScene");
    }
    // �^�C�g����ʂ�Canvas���t�F�[�h�C��������R���[�`��
    private IEnumerator FadeInCanvas()
    {
        // �A���t�@�l��1.0�����̊ԁA�t�F�[�h�C�����������s
        while (m_CurrentAlpha < 1.0f)
        {
            // �A���t�@�l�����Ԍo�߂Ƒ��x�Ɋ�Â��đ��������A0.0����1.0�ɃN�����v����
            m_CurrentAlpha += Time.deltaTime * m_EaseSpeed;
            m_CurrentAlpha = Mathf.Clamp01(m_CurrentAlpha);

            // Canvas�I�u�W�F�N�g��CanvasGroup�������Ă���ꍇ�Aalpha�v���p�e�B�Ɍ��݂̃A���t�@�l��K�p
            CanvasGroup canvasGroup = m_TitleCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = m_CurrentAlpha;
            }
            // Canvas�I�u�W�F�N�g��CanvasGroup�������Ă��Ȃ��ꍇ�AImage�R���|�[�l���g��color��alpha�Ɍ��݂̃A���t�@�l��K�p
            else
            {
                Image image = m_TitleCanvas.GetComponent<Image>();
                if (image != null)
                {
                    Color newColor = image.color;
                    newColor.a = m_CurrentAlpha;
                    image.color = newColor;
                }
            }

            yield return null;
        }

        // ���[�v�𔲂�����A�ŏI�I�ɃA���t�@�l��1.0�ɐݒ肵�āA���S�Ƀt�F�[�h�C�����I���������Ƃ��m�F
        CanvasGroup finalCanvasGroup = m_TitleCanvas.GetComponent<CanvasGroup>();
        if (finalCanvasGroup != null)
        {
            finalCanvasGroup.alpha = 1.0f;
            m_Fade.SetActive(false);
        }
        else
        {
            Image finalImage = m_TitleCanvas.GetComponent<Image>();
            if (finalImage != null)
            {
                Color finalColor = finalImage.color;
                finalColor.a = 1.0f;
                finalImage.color = finalColor;
            }
        }
    }
}