using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowItem : MonoBehaviour
{
    // オブジェクトプールマネージャへの参照
    private ItemEffectArrowObjctPool m_RecoveryEffectObjPool;
    private void Start()
    {
        // ItemEffectObjctPool のシングルトンインスタンスを取得
        m_RecoveryEffectObjPool = ItemEffectArrowObjctPool.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();
            GameObject hitEffect = m_RecoveryEffectObjPool.GetPooledObject();
            hitEffect.transform.position = transform.position;
            hitEffect.SetActive(true);

            InventorySystem.inventorySystem.AddItemWithCurrencyCheck("Arrow", ItemType.Arrow, 1, 0);
            Destroy(gameObject);

        }
    }
}
