using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoviWolfMove : MonoBehaviour
{
    public float speed = 15f;
    private bool shouldMove = false;
   
    void Update()
    {
        // �ړ��t���O�������Ă���Έړ����������s
        if (shouldMove)
        {
            // �O�����ɑ��x15�ňړ�
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
    public void isMove()
    {
        shouldMove=true;
    }
}
