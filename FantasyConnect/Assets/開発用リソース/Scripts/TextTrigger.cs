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
        // AudioSourceがアタッチされていない場合、このスクリプトがアタッチされたオブジェクトにAudioSourceを追加する
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
    private void PlaySE()
    {
        if (m_SE)
        {
            m_SE.clip = m_SEClip;
            m_SE.Play();
        }
    }
}
