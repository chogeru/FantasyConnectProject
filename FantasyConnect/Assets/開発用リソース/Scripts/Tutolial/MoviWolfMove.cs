using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoviWolfMove : MonoBehaviour
{
    public float m_Speed = 10f;
    private bool isShouldMove = false;
   
    void Update()
    {
        if (isShouldMove)
        {
            transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
        }
    }
    public void isMove()
    {
        isShouldMove=true;
    }
}
