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

    private GameObject player;
    PlayerSystem playerSystem;
    public bool isTextEnd=false;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerSystem = player.GetComponent<PlayerSystem>();

        if (instance!=null&&instance!=this)
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
            isTextEnd = false;
            textWindowUI.SetActive(true); 
            playerSystem.isStop = true;
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
            isTextEnd=true;
            playerSystem.isStop = false;
            textWindowUI.SetActive(false);
        }
    }
}
