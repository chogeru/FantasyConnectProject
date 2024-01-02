using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class AddItemButton : MonoBehaviour
{ 
    public void OnHealingAddButtonClick()
    {
        if (InventorySystem.inventorySystem != null)
        {
            // �A�C�e����ǉ�����֐����Ăяo��
            InventorySystem.inventorySystem.AddItem("Healing", ItemType.Healing, 1);
        }
    }
    public void OnMPAddButtonClick()
    {
        if (InventorySystem.inventorySystem != null)
        {
            // �A�C�e����ǉ�����֐����Ăяo��
            InventorySystem.inventorySystem.AddItem("MP", ItemType.MP, 1);
        }
    }
    public void OnArrowAddButtonClick()
    {
        if (InventorySystem.inventorySystem != null)
        {
            // �A�C�e����ǉ�����֐����Ăяo��
            InventorySystem.inventorySystem.AddItem("Arrow", ItemType.Arrow, 1);
        }
    }
}
