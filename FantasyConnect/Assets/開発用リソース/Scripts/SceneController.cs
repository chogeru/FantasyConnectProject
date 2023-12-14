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
                    SceneManager.LoadScene("MyHouse", LoadSceneMode.Single);
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
                    SceneManager.LoadScene("GrassIandArea2", LoadSceneMode.Single);
                }
                break;
            default:
                break;
        }
    }
}
