using UnityEngine.SceneManagement;
using UnityEngine;

public class SettingCanvasButton : MonoBehaviour
{
    public void LoadFirstCity()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("FirstCity");
    }
    public void LoaadTitleScene()
    {
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("Title");
    }

    public void ExitButton()
    {
        Application.Quit();
    }
}
