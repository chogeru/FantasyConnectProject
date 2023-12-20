using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    private static TextManager instance;
    public static TextManager Instance
    { get { return instance; } }
    [SerializeField]
    private GameObject textWindowUI; 

    [SerializeField]
    private TextMeshProUGUI textMeshPro; 

    private void Awake()
    {
        if(instance!=null&&instance!=this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public void ShowText(string textToShow)
    {
        if (textWindowUI != null)
        {
            textWindowUI.SetActive(true); 

            if (textMeshPro != null)
            {
 
                textMeshPro.text = textToShow;
            }
            else
            {
                Debug.LogError("TMPが無い");
            }
        }
        else
        {
            Debug.LogError("Textウィンドウがない");
        }
    }
    public void HideText()
    {
        if (textWindowUI != null)
        {
            textWindowUI.SetActive(false);
        }
    }
}
