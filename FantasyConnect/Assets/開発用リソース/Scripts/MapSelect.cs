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

    private void Start()
    {
     
        m_DownButton.SetActive(false);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            m_MapSelectUI.SetActive(true);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_MapSelectUI.SetActive(false);
        }
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
        SceneManager.LoadScene("GrassIandArea1", LoadSceneMode.Single);
    }
    public void LoadAutumnArea()
    {
        SceneManager.LoadScene("AutumnArea1", LoadSceneMode.Single);
    }
    public void LoadSnowArea()
    {
        SceneManager.LoadScene("SnowArea1", LoadSceneMode.Single);
    }
    public void LoadWastelandArea()
    {
        SceneManager.LoadScene("WastelandArea1", LoadSceneMode.Single);
    }
    public void LoadMyHouce()
    {
        SceneManager.LoadSceneAsync("MyHouse", LoadSceneMode.Single);
    }
}
