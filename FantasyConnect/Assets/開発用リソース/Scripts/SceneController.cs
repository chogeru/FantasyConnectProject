using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    //シングルトンパターン
    public static SceneController SceneConinstance;
    public bool isHitCol=false;
    private void Awake()
    {
        if (SceneConinstance == null)
        {
            SceneConinstance = this;
            //オブジェクトを残すように
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        string currntScene=SceneManager.GetActiveScene().name;
        switch (currntScene)
        {
            case "Title":
                if (Input.anyKeyDown)
                {
                    SceneManager.LoadSceneAsync("MyHouse", LoadSceneMode.Single);
                }
                break;
            case "MyHouse":
             if(isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("FirstCity", LoadSceneMode.Single);
                }
                break;
            case "FirstCity":
                if(isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("GrassIandArea1", LoadSceneMode.Single);
                }
                break;
            case "GrassIandArea1":
                if (isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("GrassIandBossArea", LoadSceneMode.Single);
                }
                break;
            case "GrassIandBossArea":
                if (isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("AutumnArea1", LoadSceneMode.Single);
                }
                break;
            case "AutumnArea1":
                if (isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("AutumnBossArea", LoadSceneMode.Single);
                }
                break;
            case "AutumnBossArea":
                if (isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("SnowArea1", LoadSceneMode.Single);
                }
                break;
            case "SnowArea1":
                if (isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("SnowBossArea", LoadSceneMode.Single);
                }
                break;
            case "SnowBossArea":
                if (isHitCol)
                {
                    isHitCol = false;
                    SceneManager.LoadScene("WastelandArea1", LoadSceneMode.Single);
                }
                break;
            default:
                break;
        }
    }
}
