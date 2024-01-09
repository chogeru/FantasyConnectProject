using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyRote : MonoBehaviour
{
    [SerializeField]
    private float m_RotetionSpeed=1;
    private void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * m_RotetionSpeed);
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
