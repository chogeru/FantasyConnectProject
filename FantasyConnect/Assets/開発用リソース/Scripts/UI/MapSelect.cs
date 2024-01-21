using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MapSelect : MonoBehaviour
{
    [SerializeField,Header("MapSelectUI")]
    private GameObject m_MapSelectUI;
    [SerializeField,Header("アニメ-ター")]
    private Animator m_Animator;
    [SerializeField, Header("UIUpButton")]
    private GameObject m_UpButton;
    [SerializeField,Header("UIDownButton")]
    private GameObject m_DownButton;

    private GameObject player;
    private GameObject playerCam;
    PlayerSystem playerSystem;
    PlayerCameraController playerCameraController;

    private void Start()
    {
        m_DownButton.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player");
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        playerSystem=player.GetComponent<PlayerSystem>();
        playerCameraController = playerCam.GetComponent<PlayerCameraController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Cursor.visible = true;
            playerCameraController.isStop = true;
            playerSystem.isStop = true;
            m_MapSelectUI.SetActive(true);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Cursor.visible = false;
            playerCameraController.isStop = false;
            playerSystem.isStop = false;
            m_MapSelectUI.SetActive(false);
        }
    }
    public void ExitUI()
    {
        Cursor.visible=false;
        playerCameraController.isStop=false;
        playerSystem.isStop=false;
        m_MapSelectUI.SetActive(false);
    }
    public void UpUI()
    {
        m_Animator.SetBool("Up",true);
        m_Animator.SetBool("Down", false);
        m_UpButton.SetActive(false);
        m_DownButton.SetActive(true);

    }
    public void DownUI()
    {
        m_Animator.SetBool("Up", false);
        m_Animator.SetBool("Down", true);
        m_UpButton.SetActive(true);
        m_DownButton.SetActive(false);
    }
    public void LoadGrassIandArea()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("GrassIandArea1");
    }
    public void LoadAutumnArea()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("AutumnArea1");
    }
    public void LoadSnowArea()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("SnowArea1");
    }
    public void LoadWastelandArea()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("WastelandArea1");
    }
    public void LoadMyHouce()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("MyHouse");
    }
    public void LoadSecondCity()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("SecondCity");
    }
    public void LoadFirstCity()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("FirstCity");
    }
    public void LoadChallengeArea()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("ChallengeArea");
    }
    public void LoadForestArea1()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("ForestArea1");
    }
}
