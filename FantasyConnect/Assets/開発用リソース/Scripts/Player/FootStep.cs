using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour
{
    [SerializeField, Header("移動時パーティクル")]
    private GameObject footstepPrefab;
    private float m_CoolTime;
    private void Update()
    {
        m_CoolTime += Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (m_CoolTime > 0.5)
        {
            GenerateFootstep();
            m_CoolTime = 0;
        }
    }


    void GenerateFootstep()
    {
        GameObject footstep = Instantiate(footstepPrefab, transform.position, Quaternion.identity);
        Vector3 forward = transform.forward;
        forward.y = 0; // y軸方向の回転を無効にする
        footstep.transform.rotation = Quaternion.LookRotation(forward);
    }
}
