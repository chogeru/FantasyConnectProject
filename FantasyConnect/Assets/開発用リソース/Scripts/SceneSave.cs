using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneSave : MonoBehaviour
{
    private string jsonFilePath;

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Title")
        {
            SaveSceneName();
        }
    }

   public  void SaveSceneName()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "sceneData.json");

        // 現在のシーン名を取得
        string currentSceneName = SceneManager.GetActiveScene().name;

        // SceneDataオブジェクトを作成
        SceneData sceneData = new SceneData(currentSceneName);

        // シーン名をJson形式に変換
        string json = JsonUtility.ToJson(sceneData);

        // ファイルが存在しない場合は新しく作成して書き込み
        if (!File.Exists(jsonFilePath))
        {
            using (StreamWriter sw = File.CreateText(jsonFilePath))
            {
                sw.WriteLine(json);
            }
        }
        else
        {
            // Jsonファイルに保存
            File.WriteAllText(jsonFilePath, json);
        }
    }

    public void LoadSceneFromJson()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "sceneData.json");

        if (File.Exists(jsonFilePath))
        {
            // Jsonファイルからデータを読み取り
            string json = File.ReadAllText(jsonFilePath);

            // JsonデータをSceneDataオブジェクトに変換
            SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

            // 指定されたシーンをロード
            SceneController.sceneController.LoadSceneWithLoadingScreen(sceneData.sceneName);
        }
    }
    public void ResetSceneData()
    {
        if (File.Exists(jsonFilePath))
        {
            File.Delete(jsonFilePath);
        }
    }
}

[System.Serializable]
public class SceneData
{
    public string sceneName;

    public SceneData(string name)
    {
        sceneName = name;
    }
}