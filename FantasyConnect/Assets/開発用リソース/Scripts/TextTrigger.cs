using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TextTrigger : MonoBehaviour
{
    public string[] textsToDisplay;
    [SerializeField]
    private int currentIndex = 0;
    [SerializeField,Header("NPCボイス用オーディオ")]
    private AudioSource m_NPCVoice;
    [SerializeField,Header("テキスト偏移用SE")]
    private AudioSource m_TextSE;
    [Foldout("ボイス用オーディオクリップ"), Tab("音声クリップ")]
    [SerializeField,Header("会話初め時の音声クリップ")]
    private AudioClip m_ConversationStartClip;
    [SerializeField, Header("会話送り時のクリップ")]
    private AudioClip m_SecondTextVoice;
    [Foldout("SE用音声クリップ")]
    [SerializeField,Header("テキスト用クリップ")]
    private AudioClip m_TextClip;


    public void TriggerTextDisplay()
    {
        if (TextManager.Instance != null)
        {
            if (textsToDisplay != null && textsToDisplay.Length > 0)
            {
                if(currentIndex==0)
                {
                    //会話スタート時のクリップ
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
                    Debug.Log("テキストがない");
                    TextManager.Instance.HideText();

                }
            }
            else
            {
                Debug.LogError("テキストの要素がない");
                TextManager.Instance.HideText(); 
            }
        }
        else
        {
            Debug.LogError("テキストがない!!");
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
