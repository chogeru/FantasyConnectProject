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
    [SerializeField]
    private TextMeshProUGUI m_HealingItemCountText;
    [SerializeField]
    private TextMeshProUGUI m_MpItemCountText;
    [SerializeField]
    private TextMeshProUGUI m_ArrowItemCountText;
    private Dictionary<string, Item> inventory = new Dictionary<string, Item>();
    public static InventorySystem inventorySystem;
    public CurrencySystem currencySystem; // �����̃V�X�e���ւ̎Q��
    [SerializeField]
    private PlayerSystem playerSystem;

    private void Awake()
    {
        // inventorySystem �C���X�^���X������������
        inventorySystem = this;
    }
    // �A�C�e����ǉ����郁�\�b�h
    public void AddItemWithCurrencyCheck(string itemName, ItemType itemType, int quantity, int requiredCurrency)
    {
        if (currencySystem.CheckCurrency(requiredCurrency))
        {
            // �A�C�e����ǉ�����O�ɕK�v�Ȃ��������炷
            currencySystem.DeductCurrency(requiredCurrency);

            // �ȑO�Ɠ����悤�ɃA�C�e����ǉ�
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
            Debug.Log(itemName + " ���w������̂ɏ\���Ȃ���������܂���");
        }
    }

    // �A�C�e�����g�p���郁�\�b�h
    public void UseItem(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            Item item = inventory[itemName];

            switch (item.type)
            {
                case ItemType.Healing:
                    playerSystem.Recovery(500);
                    Debug.Log("HP���񕜂��܂�");
                    UpdateUI(inventory[itemName].type);

                    // �̗͂��񕜂��鏈�����L�q���邱��
                    break;
                case ItemType.MP:
                    // ������MP�A�C�e���̏������L�q����
                    // �Ⴆ�΁AMP���񕜂��鏈��
                    // ���̗�ł͒P�Ɍ������炷�����ɂȂ�܂�
                    Debug.Log("MP���񕜂��܂�");
                    UpdateUI(inventory[itemName].type);
                    // MP���񕜂��鏈�����L�q���邱��
                    break;
                case ItemType.Arrow:
                    Debug.Log("�|�̖{��");
                    UpdateUI(inventory[itemName].type);
                    break;
                default:
                    Debug.Log("���̃A�C�e���̎�ނ͖���`�ł�");
                    break;
            }

            item.amount--;
            UpdateUI(inventory[itemName].type);
            // ��������0�ɂȂ�����A�A�C�e�����C���x���g������폜����
            if (item.amount <= 0)
            {
                UpdateUI(inventory[itemName].type);
                inventory.Remove(itemName);
            }
        }
        else
        {
            Debug.Log("���̃A�C�e���̓C���x���g���ɂ���܂���");
        }

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
                // Healing�A�C�e���̌����X�V
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
                // MP�A�C�e���̌����X�V
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
