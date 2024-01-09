using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
public class SceneController : MonoBehaviour
{
    public static SceneController SceneConinstance;
    public static SceneController sceneController
    { get { return SceneConinstance; } }
    public bool isHitCol = false;

    public GameObject loadingCanvas;

    private void Awake()
    {
        if (SceneConinstance == null)
        {
            SceneConinstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        string currntScene = SceneManager.GetActiveScene().name;
        switch (currntScene)
        {
            case "Title":
                if (Input.anyKeyDown)
                {
                    LoadSceneWithLoadingScreen("TutorialScene");
                }
                break;
            case "MyHouse":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("FirstCity");
                }
                break;
            case "FirstCity":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("GrassIandArea1");
                }
                break;
            case "GrassIandArea1":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("GrassIandBossArea");
                }
                break;
            case "GrassIandBossArea":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("AutumnArea1");
                }
                break;
            case "AutumnArea1":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("AutumnBossArea");
                }
                break;
            case "AutumnBossArea":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("SnowArea1");
                }
                break;
            case "SnowArea1":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("SnowBossArea");
                }
                break;
            case "SnowBossArea":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("WastelandArea1");
                }
                break;
            case "WastelandArea1":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("WastelandBossArea");
                }
                break;
            case "TutorialScene":
                if (isHitCol)
                {
                    isHitCol = false;
                    LoadSceneWithLoadingScreen("MyHouse");
                }
                break;
            default:
                break;
        }
    }

    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(true);
            Slider loadingSlider = loadingCanvas.GetComponentInChildren<Slider>();

            if (loadingSlider != null)
            {
                StartCoroutine(LoadSceneAsync(sceneName, loadingSlider));
            }
        }
    }

    IEnumerator LoadSceneAsync(string sceneName, Slider loadingSlider)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); 
            loadingSlider.value = progress;

            yield return null;
        }

        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
        }
    }
}
