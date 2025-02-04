using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class CurrencyCounts
{
    /// <summary>
    /// Json保存用変数
    /// </summary>
    public int CurrencyCount;
}
public class CurrencySystem : MonoBehaviour
{
    CurrencySystem currencySystem;
    public int m_currencyAmount;
    public TextMeshProUGUI currencyText;
    private void Start()
    {
        LoadCurrencyFromJson();
        UpdateCurrencyText();
    }

    public bool CheckCurrency(int requiredAmount)
    {
        return m_currencyAmount >= requiredAmount;
    }
    public void DeductCurrency(int amount)
    {
        if (m_currencyAmount >= amount)
        {
            m_currencyAmount -= amount;
            SaveCurrencyToJson();
        //    Debug.Log(amount + " のお金を減らした。残高: " + m_currencyAmount);
        }
        else
        {
            Debug.Log("お金が足りない");
        }
    }
    public void UpdateCurrencyText()
    {
        if (currencyText != null)
        {
            currencyText.text = m_currencyAmount.ToString();
        }
    }
    public void SaveCurrencyToJson()
    {
        CurrencyCounts currencyCounts = new CurrencyCounts();
        currencyCounts.CurrencyCount = m_currencyAmount;
        string json = JsonUtility.ToJson(currencyCounts);
        string filePath = Path.Combine(Application.persistentDataPath, "currencyAmount.json");
        File.WriteAllText(filePath, json);
    }
    public void LoadCurrencyFromJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "currencyAmount.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CurrencyCounts currencyCounts = JsonUtility.FromJson<CurrencyCounts>(json);
            if (currencyCounts != null)
            {
                m_currencyAmount = currencyCounts.CurrencyCount;
                UpdateCurrencyText();
            }
            else
            {
                Debug.Log("お金がほぞんされていない");
            }

        }
    }

    public void ResetCurrency()
    {
        m_currencyAmount = 0;
        SaveCurrencyToJson();
    }
}
