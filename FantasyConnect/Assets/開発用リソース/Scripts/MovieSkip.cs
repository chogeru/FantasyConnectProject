using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovieSkip : MonoBehaviour
{
    private float m_TimeUntilSkippable;
    void Update()
    {
        m_TimeUntilSkippable += Time.deltaTime;
        if(m_TimeUntilSkippable>5)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SceneController.SceneConinstance.LoadSceneWithLoadingScreen("MyHouse");
            }
        }
    }
}
