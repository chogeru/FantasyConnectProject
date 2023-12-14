using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColSceneChange : MonoBehaviour
{
  SceneController sceneController;
    private GameObject m_SceneManager;
    private void Start()
    {
        m_SceneManager = GameObject.Find("SceneManager");
        sceneController = m_SceneManager.GetComponent<SceneController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            sceneController.isHitCol = true;
        }
    }
}
