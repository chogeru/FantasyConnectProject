using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class BGMManager : MonoBehaviour
{
    //シングルトンパターン
    public static BGMManager BGMm_instance;
    #region　オーディオソース
    [SerializeField,Header("オーディオソース")]
    private AudioSource m_AudioSouce;
    [Foldout("オーディオクリップ")]
    [SerializeField,Header("TitleシーンのBGM")]
    private AudioClip m_TitleBGM;
    [SerializeField,Header("MyHouseシーンのBGM")]
    private AudioClip m_MyHouseBGM;
    [SerializeField, Header("FirstCityシーンのBGM")]
    private AudioClip m_FirstCityBGM;
    [SerializeField, Header("GrassIandAreaのBGM")]
    private AudioClip m_GrassIandAreaBGM;
    [SerializeField, Header("GrassIandBossAreaのBGM")]
    private AudioClip m_GrassIandBossAreaBGM;
    [SerializeField, Header("AutumnAreaのBGM")]
    private AudioClip m_AutumnAreaBGM;
    [SerializeField, Header("AutumnBossAreaのBGM")]
    private AudioClip m_AutumnBossAreaBGM;
    [SerializeField, Header("SnowArea1のBGM")]
    private AudioClip m_SnowArea1BGM;
    [SerializeField, Header("SnowBossAreaのBGM")]
    private AudioClip m_SnowBossAreaBGM;
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
                m_AudioSouce.clip = m_TitleBGM;
                m_AudioSouce.Play();
                break;
            case "MyHouse":
                m_AudioSouce.clip = m_MyHouseBGM;
                m_AudioSouce.Play();
                break;
            case "FirstCity":
                m_AudioSouce.clip = m_FirstCityBGM;
                m_AudioSouce.Play();
                break;
            case "GrassIandArea1":
                m_AudioSouce.clip = m_GrassIandAreaBGM;
                m_AudioSouce.Play();
                break;
            case "GrassIandBossArea":
                m_AudioSouce.clip = m_GrassIandBossAreaBGM;
                m_AudioSouce.Play();
                break;
            case "AutumnArea1":
                m_AudioSouce.clip = m_AutumnAreaBGM;
                m_AudioSouce.Play();
                break;
            case "AutumnBossArea":
                m_AudioSouce.clip = m_AutumnBossAreaBGM;
                m_AudioSouce.Play();
                break;
            case "SnowArea1":
                m_AudioSouce.clip = m_SnowArea1BGM;
                m_AudioSouce.Play();
                break;
            case "SnowBossArea":
                m_AudioSouce.clip = m_SnowBossAreaBGM;
                m_AudioSouce.Play();
                break;
            default:
                break;
        }
    }
}
