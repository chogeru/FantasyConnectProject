using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Canvas;

    public void ActiveCanvas()
    {
        m_Canvas.SetActive(true);
    }
    public void CloseCanvas()
    {
        m_Canvas.SetActive(false);
    }
}
