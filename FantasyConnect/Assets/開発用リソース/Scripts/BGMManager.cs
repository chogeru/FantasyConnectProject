using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class BGMManager : MonoBehaviour
{
    //シングルトンパターン
    public static BGMManager BGMm_instance;
    #region　オーディオソース
    [Foldout("オーディオソース")]
    [SerializeField,Header("TitleシーンのBGM")]
    private AudioSource m_TitleBGM;
    [SerializeField,Header("MyHouseシーンのBGM")]
    private AudioSource m_MyHouseBGM;
    [SerializeField, Header("FirstCityシーンのBGM")]
    private AudioSource m_FirstCityBGM;
    [SerializeField, Header("GrassIandAreaのBGM")]
    private AudioSource m_GrassIandAreaBGM;
    [SerializeField, Header("GrassIandBossAreaのBGM")]
    private AudioSource m_GrassIandBossAreaBGM;
    [SerializeField, Header("AutumnAreaのBGM")]
    private AudioSource m_AutumnAreaBGM;
    [SerializeField, Header("AutumnBossAreaのBGM")]
    private AudioSource m_AutumnBossAreaBGM;
    #endregion
    private void Awake()
    {
        if(BGMm_instance == null)
        {
            BGMm_instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        string sceneName=SceneManager.GetActiveScene().name;
        PlayBGMByScene(sceneName);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        // イベントリスナーを解除する
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新しいシーンがロードされたらそのシーンに適したBGMを再生する
        PlayBGMByScene(scene.name);
    }
    private void PlayBGMByScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Title":
                m_FirstCityBGM.Stop();
                m_MyHouseBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_GrassIandBossAreaBGM.Stop();
                m_TitleBGM.Play();
                m_AutumnBossAreaBGM.Stop();
                m_AutumnAreaBGM.Stop();
                break;
            case "MyHouse":
                m_FirstCityBGM.Stop();
                m_TitleBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_GrassIandBossAreaBGM.Stop();
                m_MyHouseBGM.Play();
                m_AutumnBossAreaBGM.Stop();
                m_AutumnAreaBGM.Stop();
                break;
            case "FirstCity":
                m_FirstCityBGM.Play();
                m_TitleBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_GrassIandBossAreaBGM.Stop();
                m_AutumnBossAreaBGM.Stop();
                m_MyHouseBGM.Stop();
                m_AutumnAreaBGM.Stop();
                break;
            case "GrassIandArea1":
                m_FirstCityBGM.Stop();
                m_TitleBGM.Stop();
                m_MyHouseBGM.Stop();
                m_GrassIandAreaBGM.Play();
                m_GrassIandBossAreaBGM.Stop();
                m_AutumnBossAreaBGM.Stop();
                m_AutumnAreaBGM.Stop();
                break;
            case "GrassIandBossArea":
                m_FirstCityBGM.Stop();
                m_TitleBGM.Stop();
                m_MyHouseBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_GrassIandBossAreaBGM.Play();
                m_AutumnBossAreaBGM.Stop();
                m_AutumnAreaBGM.Stop();
                break;
            case "AutumnArea1":
                m_FirstCityBGM.Stop();
                m_TitleBGM.Stop();
                m_MyHouseBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_GrassIandBossAreaBGM.Stop();
                m_AutumnBossAreaBGM.Stop();
                m_AutumnAreaBGM.Play();
                break;
            case "AutumnBossArea":
                m_FirstCityBGM.Stop();
                m_TitleBGM.Stop();
                m_MyHouseBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_GrassIandBossAreaBGM.Stop();
                m_AutumnAreaBGM.Stop();
                m_AutumnBossAreaBGM.Play();
                break;
            default:
                break;
        }
    }
}
