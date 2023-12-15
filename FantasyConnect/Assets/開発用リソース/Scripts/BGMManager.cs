using UnityEngine;
using UnityEngine.SceneManagement;
public class BGMManager : MonoBehaviour
{
    //�V���O���g���p�^�[��
    public static BGMManager BGMm_instance;
    [SerializeField,Header("Title�V�[����BGM")]
    private AudioSource m_TitleBGM;
    [SerializeField,Header("MyHouse�V�[����BGM")]
    private AudioSource m_MyHouseBGM;
    [SerializeField, Header("FirstCity�V�[����BGM")]
    private AudioSource m_FirstCityBGM;
    [SerializeField, Header("GrassIandArea")]
    private AudioSource m_GrassIandAreaBGM;
    private void Awake()
    {
        if(BGMm_instance == null)
        {
            BGMm_instance = this;
            //�I�u�W�F�N�g���c���悤��
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
        // �C�x���g���X�i�[��o�^���A�V�[�������[�h���ꂽ�Ƃ��ɌĂ΂�鏈�����`����
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
