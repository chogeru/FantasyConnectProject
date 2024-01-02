using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddItemButton : MonoBehaviour
{
    public InventorySystem inventorySystem; // InventorySystemスクリプトへの参照を保持

    [SerializeField, Header("Itemの名前")]
    private string m_AddItemName;
    public void OnHealingItemClick()
    {
        // アイテムを追加する関数を呼び出す
        inventorySystem.AddItem(m_AddItemName, ItemType.Healing, 1);
    }
}
