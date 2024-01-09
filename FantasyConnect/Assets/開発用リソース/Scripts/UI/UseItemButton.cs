using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemButton : MonoBehaviour
{
    [SerializeField]
    InventorySystem inventorySystem;
    [SerializeField]
    PlayerSystem playerSystem;
    public void OnHealingUseButtonClick()
    {
        if (playerSystem.m_CurrentHp < playerSystem.m_MaxHp)
        {
            InventorySystem.inventorySystem.UseItem("Healing", 1);
        }
    }
    public void OnMPUseButtonClick()
    {
        if (playerSystem.m_MP < playerSystem.m_MaxMP)
        {
            InventorySystem.inventorySystem.UseItem("MP", 1);
        }
    }
}
