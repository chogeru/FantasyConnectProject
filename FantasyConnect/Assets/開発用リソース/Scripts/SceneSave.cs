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

        // ���݂̃V�[�������擾
        string currentSceneName = SceneManager.GetActiveScene().name;

        // SceneData�I�u�W�F�N�g���쐬
        SceneData sceneData = new SceneData(currentSceneName);

        // �V�[������Json�`���ɕϊ�
        string json = JsonUtility.ToJson(sceneData);

        // �t�@�C�������݂��Ȃ��ꍇ�͐V�����쐬���ď�������
        if (!File.Exists(jsonFilePath))
        {
            using (StreamWriter sw = File.CreateText(jsonFilePath))
            {
                sw.WriteLine(json);
            }
        }
        else
        {
            // Json�t�@�C���ɕۑ�
            File.WriteAllText(jsonFilePath, json);
        }
    }

    public void LoadSceneFromJson()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "sceneData.json");

        if (File.Exists(jsonFilePath))
        {
            // Json�t�@�C������f�[�^��ǂݎ��
            string json = File.ReadAllText(jsonFilePath);

            // Json�f�[�^��SceneData�I�u�W�F�N�g�ɕϊ�
            SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

            // �w�肳�ꂽ�V�[�������[�h
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