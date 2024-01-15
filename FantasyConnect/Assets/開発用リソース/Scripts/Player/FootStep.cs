using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour
{
    [SerializeField, Header("移動時パーティクル")]
    private GameObject footstepPrefab;
    private float m_CoolTime;
    private EffectFootStepObjctPool m_EffectFootObjPool;
    private void Start()
    {
        m_EffectFootObjPool = EffectFootStepObjctPool.Instance;
    }
    private void Update()
    {
        m_CoolTime += Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (m_CoolTime > 0.05)
        {
            GenerateFootstep();
            m_CoolTime = 0;
        }
    }


    void GenerateFootstep()
    {
        GameObject hitEffect = m_EffectFootObjPool.GetPooledObject();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.rotation = Quaternion.identity;
        hitEffect.SetActive(true);

      //  GameObject footstep = Instantiate(footstepPrefab, transform.position, Quaternion.identity);
        Vector3 forward = transform.forward;
        forward.y = 0; // y軸方向の回転を無効にする
        hitEffect.transform.rotation = Quaternion.LookRotation(forward);
    }
}
