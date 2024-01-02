using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public enum ItemType
{
    Healing,
    MP,
    Arrow
}
public class Item
{
    public ItemType type;
    public int amount;

    public Item(ItemType itemType, int itemamount)
    {
        type = itemType;
        amount = itemamount;
    }
}

public class InventorySystem : MonoBehaviour
{
    public TextMeshProUGUI m_HealingItemCountText; // Healingアイテムの個数を表示するTMPProテキスト
    public TextMeshProUGUI m_MpItemCountText;
    public TextMeshProUGUI m_ArrowItemCountText;
    private Dictionary<string, Item> inventory = new Dictionary<string, Item>();
    public static InventorySystem inventorySystem;

    private void Awake()
    {
        // inventorySystem インスタンスを初期化する
        inventorySystem = this;
    }
    // アイテムを追加するメソッド
    public void AddItem(string itemName, ItemType itemType, int quantity)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName].amount += quantity; // アイテムが既に存在する場合、数量を追加
        }
        else
        {
            inventory.Add(itemName, new Item(itemType, quantity)); // アイテムが存在しない場合、新しく追加
        }
        UpdateUI(itemType);

    }

    // アイテムを使用するメソッド
    public void UseItem(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            Item item = inventory[itemName];

            switch (item.type)
            {
                case ItemType.Healing:
                    // ここに回復アイテムの処理を記述する
                    // 例えば、体力を回復する処理
                    // この例では単に個数を減らすだけになります
                    Debug.Log("HPを回復します");
                    UpdateUI(inventory[itemName].type);

                    // 体力を回復する処理を記述すること
                    break;
                case ItemType.MP:
                    // ここにMPアイテムの処理を記述する
                    // 例えば、MPを回復する処理
                    // この例では単に個数を減らすだけになります
                    Debug.Log("MPを回復します");
                    UpdateUI(inventory[itemName].type);
                    // MPを回復する処理を記述すること
                    break;
                case ItemType.Arrow:
                    Debug.Log("弓の本数");
                    UpdateUI(inventory[itemName].type);
                    break;
                default:
                    Debug.Log("そのアイテムの種類は未定義です");
                    break;
            }

            item.amount--;

            // もし個数が0になったら、アイテムをインベントリから削除する
            if (item.amount <= 0)
            {
                UpdateUI(inventory[itemName].type);
                inventory.Remove(itemName);
            }
        }
        else
        {
            Debug.Log("そのアイテムはインベントリにありません");
        }

    }
    private void UpdateUI(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Healing:
                // Healingアイテムの個数を更新
                if (m_HealingItemCountText != null && inventory.ContainsKey("Healing"))
                {
                    m_HealingItemCountText.text = inventory["Healing"].amount.ToString();
                }
                else if (m_HealingItemCountText != null)
                {
                    m_HealingItemCountText.text = "0"; 
                }
                break;

            case ItemType.MP:
                // MPアイテムの個数を更新
                if (m_MpItemCountText != null && inventory.ContainsKey("MP"))
                {
                    m_MpItemCountText.text = inventory["MP"].amount.ToString(); 
                }
                else if (m_MpItemCountText != null)
                {
                    m_MpItemCountText.text = "0"; 
                }
                break;
            case ItemType.Arrow:
                if(m_ArrowItemCountText!=null&&inventory.ContainsKey("Arrow"))
                {
                    m_ArrowItemCountText.text = inventory["Arrow"].amount.ToString();
                }
                else if(m_ArrowItemCountText != null)
                {
                    m_ArrowItemCountText.text = "0";
                }
                break;
            default:
                break;
        }
    }
}
