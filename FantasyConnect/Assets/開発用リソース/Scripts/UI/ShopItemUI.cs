using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_HealingItemCountTxte;
    [SerializeField]
    private TextMeshProUGUI m_MPItemCountText;
    [SerializeField]
    private TextMeshProUGUI m_ArrowItemCountText;

    private void Update()
    {
        GetPlayerItemCount();
    }
    private void GetPlayerItemCount()
    {
        if (InventorySystem.inventorySystem != null && m_HealingItemCountTxte != null)
        {
            int healingItemCount = InventorySystem.inventorySystem.GetItemCount("Healing");
            m_HealingItemCountTxte.text = healingItemCount.ToString();
        }
        if (InventorySystem.inventorySystem != null && m_MPItemCountText != null)
        {
            int mpItemCount = InventorySystem.inventorySystem.GetItemCount("MP");
            m_MPItemCountText.text = mpItemCount.ToString();
        }
        if (InventorySystem.inventorySystem != null && m_ArrowItemCountText != null)
        {
            int arrowItemCount = InventorySystem.inventorySystem.GetItemCount("Arrow");
            m_ArrowItemCountText.text = arrowItemCount.ToString();
        }
    }
}
