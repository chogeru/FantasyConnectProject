using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddItemButton : MonoBehaviour
{
    public InventorySystem inventorySystem; // InventorySystem�X�N���v�g�ւ̎Q�Ƃ�ێ�

    [SerializeField, Header("Item�̖��O")]
    private string m_AddItemName;
    public void OnHealingItemClick()
    {
        // �A�C�e����ǉ�����֐����Ăяo��
        inventorySystem.AddItem(m_AddItemName, ItemType.Healing, 1);
    }
}
