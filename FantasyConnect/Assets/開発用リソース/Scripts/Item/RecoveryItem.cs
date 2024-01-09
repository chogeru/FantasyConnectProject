using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoveryItem : MonoBehaviour
{
    [SerializeField, Header("回復力")]
    private int m_Recover;
    [SerializeField, Header("接触時のエフェクト")]
    private GameObject m_HitEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerSystem playerSystem=other.GetComponent<PlayerSystem>();
            Instantiate(m_HitEffect,transform.position,Quaternion.identity);
            playerSystem.HpRecovery(m_Recover);
            Destroy(gameObject);
        }
    }
}

