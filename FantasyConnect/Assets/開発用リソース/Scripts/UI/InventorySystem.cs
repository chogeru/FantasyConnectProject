using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public enum ItemType
{
    Healing,
    MP,
    Arrow
}
[System.Serializable]
public class ItemCounts
{
    public int ArrowCount;
    public int MPCount;
    public int HealingCount;
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
    [SerializeField]
    private TextMeshProUGUI m_HealingItemCountText;
    [SerializeField]
    private TextMeshProUGUI m_MpItemCountText;
    [SerializeField]
    private TextMeshProUGUI m_ArrowItemCountText;
    [SerializeField, Header("Arrow表示テキスト")]
    private TextMeshProUGUI m_ArrowText;
    public Dictionary<string, Item> inventory = new Dictionary<string, Item>();
    public static InventorySystem inventorySystem;
    public CurrencySystem currencySystem; // お金のシステムへの参照
    [SerializeField]
    private PlayerSystem playerSystem;

    private void Awake()
    {
        // inventorySystem インスタンスを初期化する
        inventorySystem = this;
    }
    // アイテムを追加するメソッド
    public void AddItemWithCurrencyCheck(string itemName, ItemType itemType, int quantity, int requiredCurrency)
    {
        if (currencySystem.CheckCurrency(requiredCurrency))
        {
            // アイテムを追加する前に必要なお金を減らす
            currencySystem.DeductCurrency(requiredCurrency);

            // 以前と同じようにアイテムを追加
            if (inventory.ContainsKey(itemName))
            {
                inventory[itemName].amount += quantity;
            }
            else
            {
                inventory.Add(itemName, new Item(itemType, quantity));
            }

            UpdateUI(itemType);
            currencySystem.UpdateCurrencyText();
        }
        else
        {
            Debug.Log(itemName + " を購入するのに十分なお金がありません");
        }
        SaveItemCountsOnChange();

    }

    // アイテムを使用するメソッド
    public void UseItem(string itemName,int quantity)
    {
        if (inventory.ContainsKey(itemName))
        {
            Item item = inventory[itemName];

            switch (item.type)
            {
                case ItemType.Healing:
                    playerSystem.HpRecovery(500);
                    item.amount-=quantity;
                    Debug.Log("HPを回復します");
                    UpdateUI(inventory[itemName].type);
                    break;
                case ItemType.MP:
                    Debug.Log("MPを回復します");
                    playerSystem.MPRecovery(100);
                    item.amount -= quantity;
                    UpdateUI(inventory[itemName].type);

                    break;
                case ItemType.Arrow:
                    Debug.Log("弓の本数");
                    UpdateUI(inventory[itemName].type);
                    item.amount-=quantity;
                    break;
                default:
                    Debug.Log("そのアイテムの種類は未定義");
                    break;
            }

            UpdateUI(inventory[itemName].type);
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
        SaveItemCountsOnChange();

    }
    public int GetItemCount(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            return inventory[itemName].amount;
        }
        return 0;
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
                if(m_ArrowText != null&&inventory.ContainsKey("Arrow"))
                {
                    m_ArrowText.text = ""+inventory["Arrow"].amount.ToString();
                }
                else if(m_ArrowText != null)
                {
                    m_ArrowText.text = "X 0";
                }
                break;
            default:
                break;
        }
    }
    public void SaveItemCountsToJson()
    {
        ItemCounts counts = new ItemCounts();

        // インベントリから各アイテムの数を取得
        if (inventory.ContainsKey("Arrow"))
            counts.ArrowCount = inventory["Arrow"].amount;
        if (inventory.ContainsKey("MP"))
            counts.MPCount = inventory["MP"].amount;
        if (inventory.ContainsKey("Healing"))
            counts.HealingCount = inventory["Healing"].amount;

        // 数をJSONに変換
        string json = JsonUtility.ToJson(counts);

        // ファイルに保存（ファイルパスは必要に応じて変更してください）
        string filePath = Path.Combine(Application.persistentDataPath, "itemCounts.json");
        File.WriteAllText(filePath, json);
    }

    // JSONファイルからアイテムの数を読み込むメソッド
    public void LoadItemCountsFromJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "itemCounts.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            // JSONをItemCountsオブジェクトにデシリアライズ
            ItemCounts counts = JsonUtility.FromJson<ItemCounts>(json);

            // countsがnullでないことを確認し、インベントリの数を更新
            if (counts != null)
            {
                // インベントリにキーが存在するかチェックし、存在する場合は数を更新
                if (inventory.ContainsKey("Arrow"))
                    inventory["Arrow"].amount = counts.ArrowCount;
                else
                    inventory.Add("Arrow", new Item(ItemType.Arrow, counts.ArrowCount));

                if (inventory.ContainsKey("MP"))
                    inventory["MP"].amount = counts.MPCount;
                else
                    inventory.Add("MP", new Item(ItemType.MP, counts.MPCount));

                if (inventory.ContainsKey("Healing"))
                    inventory["Healing"].amount = counts.HealingCount;
                else
                    inventory.Add("Healing", new Item(ItemType.Healing, counts.HealingCount));

                // ロード後にUIを更新する
                UpdateUI(ItemType.Arrow);
                UpdateUI(ItemType.MP);
                UpdateUI(ItemType.Healing);
            }
            else
            {
                Debug.LogError("Failed to deserialize ItemCounts from JSON.");
            }
        }
        else
        {
            Debug.LogWarning("Item counts file does not exist.");
        }
    }


    // アイテムの数が変更された時に呼び出すメソッド
    public void SaveItemCountsOnChange()
    {
        SaveItemCountsToJson();
    }

    // ゲーム開始時に保存されたアイテムの数を読み込む
    void Start()
    {
        LoadItemCountsFromJson();
    }
}
