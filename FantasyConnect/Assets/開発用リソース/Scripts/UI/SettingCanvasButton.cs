using UnityEngine.SceneManagement;
using UnityEngine;

public class SettingCanvasButton : MonoBehaviour
{
    [SerializeField,Header("���z�V�X�e��")]
    CurrencySystem currencySystem;
    [SerializeField,Header("�C���x���g���V�X�e��")]
    InventorySystem inventorySystem;
    [SerializeField, Header("�V�[���f�[�^")]
    private SceneSave sceneSave;
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

    public void GameReset()
    {
        currencySystem.ResetCurrency();
        inventorySystem.ResetItem();
        sceneSave.ResetSceneData();
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("Title");
    }
    public void GameSave()
    {
        currencySystem.SaveCurrencyToJson();
        inventorySystem.SaveItemCountsToJson();
        sceneSave.SaveSceneName();
    }
}
