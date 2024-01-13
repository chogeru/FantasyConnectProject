using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class BGMManager : MonoBehaviour
{
    //�V���O���g���p�^�[��
    public static BGMManager BGMm_instance;
    [Tab("�I�[�f�B�I�\�[�X")]
    #region�@�I�[�f�B�I�\�[�X
    [SerializeField,Header("�I�[�f�B�I�\�[�X")]
    private AudioSource m_AudioSouce;
    [Foldout("�I�[�f�B�I�N���b�v"),Tab("�I�[�f�B�I�N���b�v")]
    [SerializeField,Header("Title�V�[����BGM")]
    private AudioClip m_TitleBGM;
    [SerializeField,Header("MyHouse�V�[����BGM")]
    private AudioClip m_MyHouseBGM;
    [SerializeField, Header("FirstCity�V�[����BGM")]
    private AudioClip m_FirstCityBGM;
    [SerializeField, Header("GrassIandArea��BGM")]
    private AudioClip m_GrassIandAreaBGM;
    [SerializeField, Header("GrassIandBossArea��BGM")]
    private AudioClip m_GrassIandBossAreaBGM;
    [SerializeField, Header("AutumnArea��BGM")]
    private AudioClip m_AutumnAreaBGM;
    [SerializeField, Header("AutumnBossArea��BGM")]
    private AudioClip m_AutumnBossAreaBGM;
    [SerializeField, Header("SnowArea1��BGM")]
    private AudioClip m_SnowArea1BGM;
    [SerializeField, Header("SnowBossArea��BGM")]
    private AudioClip m_SnowBossAreaBGM;
    [SerializeField, Header("WastelandArea1��BGM")]
    private AudioClip m_WastelandArea1BGM;
    [SerializeField, Header("WastelandBossArea��BGM")]
    private AudioClip m_WastelandBossAreaBGM;
    [SerializeField, Header("SecondCity��BGM")]
    private AudioClip m_SecondCityBGM;
    [SerializeField, Header("ChallengeArea��BGM")]
    private AudioClip m_ChallengeAreaBGM;
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
            case "WastelandArea1":
                m_AudioSouce.clip = m_WastelandArea1BGM;
                m_AudioSouce.Play();
                break;
            case "WastelandBossArea":
                m_AudioSouce.clip= m_WastelandBossAreaBGM;
                m_AudioSouce.Play();
                break;
            case "SecondCity":
                m_AudioSouce.clip = m_SecondCityBGM;
                m_AudioSouce.Play();
                break;
            case "TutorialScene":
                m_AudioSouce.Stop();
                break;
            case "ChallengeArea":
                m_AudioSouce.clip = m_ChallengeAreaBGM;
                m_AudioSouce.Play();
                break;
            default:
                break;
        }
    }
}
