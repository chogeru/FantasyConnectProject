using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    public string[] textsToDisplay;
    private int currentIndex = 0;
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
                }
                else
                {
                    Debug.Log("No more texts to display.");

                }
            }
            else
            {
                Debug.LogError("Text array is empty!");
            }
        }
        else
        {
            Debug.LogError("�e�L�X�g���Ȃ�!!");
        }
        if (currentIndex >= textsToDisplay.Length)
        {
            TextManager.Instance.HideText(); // �e�L�X�g��\���̏��������s
        }
    }

    public void ResetTextIndex()
    {
        currentIndex = 0;
    }
}
