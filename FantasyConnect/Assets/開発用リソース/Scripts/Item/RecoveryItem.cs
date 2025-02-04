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
    [SerializeField,Header("アイテムの種類")]
    private ItemType m_itemType;
    [SerializeField, Header("HP回復力")]
    private int m_HPRecover;
    [SerializeField,Header("MP回復力")]
    private int m_MPRecover;
    [SerializeField, Header("接触時のエフェクト")]
    private GameObject m_HitEffect;

    // オブジェクトプールマネージャへの参照
    private ItemEffectRecoveryObjctPool m_RecoveryEffectObjPool;
    private ItemEffectMPObjctPool m_MpEffectObjctPool;
    private void Start()
    {
        // ItemEffectObjctPool のシングルトンインスタンスを取得
        m_RecoveryEffectObjPool = ItemEffectRecoveryObjctPool.Instance;
        m_MpEffectObjctPool = ItemEffectMPObjctPool.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (m_itemType == ItemType.HP)
            {
                PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();

                // オブジェクトプールからエフェクトを取得
                GameObject hitEffect = m_RecoveryEffectObjPool.GetPooledObject();
                hitEffect.transform.position = transform.position;
                hitEffect.SetActive(true);

                playerSystem.HpRecovery(m_HPRecover);
                Destroy(gameObject);
            }
            else if (m_itemType == ItemType.MP)
            {
                PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();

                // オブジェクトプールからエフェクトを取得
                GameObject hitEffect = m_MpEffectObjctPool.GetPooledObject();
                hitEffect.transform.position = transform.position;
                hitEffect.SetActive(true);

                playerSystem.MPRecovery(m_MPRecover);
                Destroy(gameObject);
            }
        }
    }
}

