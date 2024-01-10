using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoveryItem : MonoBehaviour
{
    enum ItemType
    {
        HP,
        MP,
    }
    [SerializeField,Header("�A�C�e���̎��")]
    private ItemType m_itemType;
    [SerializeField, Header("HP�񕜗�")]
    private int m_HPRecover;
    [SerializeField,Header("MP�񕜗�")]
    private int m_MPRecover;
    [SerializeField, Header("�ڐG���̃G�t�F�N�g")]
    private GameObject m_HitEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (m_itemType == ItemType.HP)
            {
                PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();
                Instantiate(m_HitEffect, transform.position, Quaternion.identity);
                playerSystem.HpRecovery(m_HPRecover);
                Destroy(gameObject);
            }
            if(m_itemType == ItemType.MP)
            {
                PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();
                Instantiate(m_HitEffect, transform.position, Quaternion.identity);
                playerSystem.MPRecovery(m_MPRecover);
                Destroy(gameObject);

            }
        }
    }
}

