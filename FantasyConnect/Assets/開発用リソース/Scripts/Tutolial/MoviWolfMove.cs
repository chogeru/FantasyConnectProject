using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoviWolfMove : MonoBehaviour
{
    public float speed = 15f;
    private bool shouldMove = false;
   
    void Update()
    {
        // 移動フラグが立っていれば移動処理を実行
        if (shouldMove)
        {
            // 前方向に速度15で移動
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
    public void isMove()
    {
        shouldMove=true;
    }
}
