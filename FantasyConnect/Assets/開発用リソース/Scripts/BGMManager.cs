using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class BGMManager : MonoBehaviour
{
    //�V���O���g���p�^�[��
    public static BGMManager BGMm_instance;
    #region�@�I�[�f�B�I�\�[�X
    [Foldout("�I�[�f�B�I�\�[�X")]
    [SerializeField,Header("Title�V�[����BGM")]
    private AudioSource m_TitleBGM;
    [SerializeField,Header("MyHouse�V�[����BGM")]
    private AudioSource m_MyHouseBGM;
    [SerializeField, Header("FirstCity�V�[����BGM")]
    private AudioSource m_FirstCityBGM;
    [SerializeField, Header("GrassIandArea��BGM")]
    private AudioSource m_GrassIandAreaBGM;
    [SerializeField, Header("GrassIandBossArea��BGM")]
    private AudioSource m_GrassIandBossAreaBGM;
    [SerializeField, Header("AutumnArea��BGM")]
    private AudioSource m_AutumnAreaBGM;
    [SerializeField, Header("AutumnBossArea��BGM")]
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
        // �C�x���g���X�i�[����������
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �V�����V�[�������[�h���ꂽ�炻�̃V�[���ɓK����BGM���Đ�����
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
