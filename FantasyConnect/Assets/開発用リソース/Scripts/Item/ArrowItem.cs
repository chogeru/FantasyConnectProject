using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowItem : MonoBehaviour
{
    // オブジェクトプールマネージャへの参照
    private ItemEffectArrowObjctPool m_ArrowEffectObjPool;
    private void Start()
    {
        // ItemEffectObjctPool のシングルトンインスタンスを取得
        m_ArrowEffectObjPool = ItemEffectArrowObjctPool.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();
            GameObject hitEffect = m_ArrowEffectObjPool.GetPooledObject();
            hitEffect.transform.position = transform.position;
            hitEffect.SetActive(true);
            playerSystem.ItemHitSound();
            InventorySystem.inventorySystem.AddItemWithCurrencyCheck("Arrow", ItemType.Arrow, 1, 0);
            Destroy(gameObject);

        }
    }
}
