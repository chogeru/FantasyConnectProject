using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TextTrigger : MonoBehaviour
{
    public string[] textsToDisplay;
    [SerializeField]
    private int currentIndex = 0;
    [SerializeField]
    private AudioSource m_SE;
    [SerializeField]
    private AudioClip m_SEClip;
    private void Start()
    {
        // AudioSource���A�^�b�`����Ă��Ȃ��ꍇ�A���̃X�N���v�g���A�^�b�`���ꂽ�I�u�W�F�N�g��AudioSource��ǉ�����
        if (!m_SE)
        {
            m_SE = gameObject.AddComponent<AudioSource>();
        }
    }
    public void TriggerTextDisplay()
    {
        if (TextManager.Instance != null)
        {
            if (textsToDisplay != null && textsToDisplay.Length > 0)
            {
                if (currentIndex < textsToDisplay.Length)
                {
                    TextManager.Instance.ShowText(textsToDisplay[currentIndex]);
                    Debug.Log(textsToDisplay[currentIndex]);
                    currentIndex++;
                    PlaySE();
                }
                else
                {
                    Debug.Log("�e�L�X�g���Ȃ�");
                    TextManager.Instance.HideText();

                }
            }
            else
            {
                Debug.LogError("�e�L�X�g�̗v�f���Ȃ�");
                TextManager.Instance.HideText(); 
            }
        }
        else
        {
            Debug.LogError("�e�L�X�g���Ȃ�!!");
        }
        if (currentIndex > textsToDisplay.Length)
        {
            TextManager.Instance.HideText(); 
        }
    }

    public void ResetTextIndex()
    {
        currentIndex = 0;
    }
    private void PlaySE()
    {
        if (m_SE)
        {
            m_SE.clip = m_SEClip;
            m_SE.Play();
        }
    }
}
