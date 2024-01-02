using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencySystem : MonoBehaviour
{
    CurrencySystem currencySystem;
    public int m_currencyAmount;
    public TextMeshProUGUI currencyText;
    private void Start()
    {
        UpdateCurrencyText();
    }
    public bool CheckCurrency(int requiredAmount)
    {
        return m_currencyAmount >= requiredAmount;
    }
    public void DeductCurrency(int amount)
    {
        if(m_currencyAmount>=amount)
        {
            m_currencyAmount -= amount;

            Debug.Log(amount + " のお金を減らしました。残高: " + m_currencyAmount);
        }
        else
        {
            Debug.Log("お金が足りません！");
        }
    }
    public void UpdateCurrencyText()
    {
        if (currencyText != null)
        {
            currencyText.text = m_currencyAmount.ToString();
        }
    }
}
