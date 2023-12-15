using UnityEngine;
using UnityEngine.SceneManagement;
public class BGMManager : MonoBehaviour
{
    //シングルトンパターン
    public static BGMManager BGMm_instance;
    [SerializeField,Header("TitleシーンのBGM")]
    private AudioSource m_TitleBGM;
    [SerializeField,Header("MyHouseシーンのBGM")]
    private AudioSource m_MyHouseBGM;
    [SerializeField, Header("FirstCityシーンのBGM")]
    private AudioSource m_FirstCityBGM;
    [SerializeField, Header("GrassIandArea")]
    private AudioSource m_GrassIandAreaBGM;
    private void Awake()
    {
        if(BGMm_instance == null)
        {
            BGMm_instance = this;
            //オブジェクトを残すように
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
        // イベントリスナーを登録し、シーンがロードされたときに呼ばれる処理を定義する
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
                m_TitleBGM.Play();
                break;
            case "MyHouse":
                m_FirstCityBGM.Stop();
                m_TitleBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_MyHouseBGM.Play();
                break;
            case "FirstCity":
                m_FirstCityBGM.Play();
                m_TitleBGM.Stop();
                m_GrassIandAreaBGM.Stop();
                m_MyHouseBGM.Stop();
                break;
            case "GrassIandArea1":
                m_FirstCityBGM.Stop();
                m_TitleBGM.Stop();
                m_MyHouseBGM.Stop();
                m_GrassIandAreaBGM.Play();
                break;
            default:
                break;
        }
    }
}
