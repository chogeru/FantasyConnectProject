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
            // �K�v�Ȃ����̗ʂ��w�肵�ĐV�������\�b�h���g�p����
            InventorySystem.inventorySystem.AddItemWithCurrencyCheck("Healing", ItemType.Healing, 1,500);
        }
    }

    public void OnMPAddButtonClick()
    {
        if (InventorySystem.inventorySystem != null)
        {
            // �K�v�Ȃ����̗ʂ��w�肵�ĐV�������\�b�h���g�p����
            InventorySystem.inventorySystem.AddItemWithCurrencyCheck("MP", ItemType.MP, 1, 500);
        }
    }

    public void OnArrowAddButtonClick()
    {
        if (InventorySystem.inventorySystem != null)
        {
            // �K�v�Ȃ����̗ʂ��w�肵�ĐV�������\�b�h���g�p����
            InventorySystem.inventorySystem.AddItemWithCurrencyCheck("Arrow", ItemType.Arrow, 1, 250);
        }
    }
}
