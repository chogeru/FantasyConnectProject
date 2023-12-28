using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TextTrigger : MonoBehaviour
{
    public string[] textsToDisplay;
    [SerializeField]
    private int currentIndex = 0;
    [SerializeField,Header("NPC�{�C�X�p�I�[�f�B�I")]
    private AudioSource m_NPCVoice;
    [SerializeField,Header("�e�L�X�g�ΈڗpSE")]
    private AudioSource m_TextSE;
    [Foldout("�{�C�X�p�I�[�f�B�I�N���b�v"), Tab("�����N���b�v")]
    [SerializeField,Header("��b���ߎ��̉����N���b�v")]
    private AudioClip m_ConversationStartClip;
    [SerializeField, Header("��b���莞�̃N���b�v")]
    private AudioClip m_SecondTextVoice;
    [Foldout("SE�p�����N���b�v")]
    [SerializeField,Header("�e�L�X�g�p�N���b�v")]
    private AudioClip m_TextClip;


    public void TriggerTextDisplay()
    {
        if (TextManager.Instance != null)
        {
            if (textsToDisplay != null && textsToDisplay.Length > 0)
            {
                if(currentIndex==0)
                {
                    //��b�X�^�[�g���̃N���b�v
                    m_NPCVoice.clip = m_ConversationStartClip;
                   m_NPCVoice.Play();
                }
                else
                {
                    m_NPCVoice.clip= m_SecondTextVoice;
                    m_NPCVoice.Play();
                }
                if (currentIndex < textsToDisplay.Length)
                {
                    TextManager.Instance.ShowText(textsToDisplay[currentIndex]);
                    Debug.Log(textsToDisplay[currentIndex]);
                    currentIndex++;
                    m_TextSE.Play();
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
}
