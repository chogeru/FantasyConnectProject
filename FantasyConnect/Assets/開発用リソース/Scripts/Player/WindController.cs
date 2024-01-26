using MagicaCloth2;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindController : MonoBehaviour
{
    [SerializeField, Header("マップに配置しているMagicWindZon")]
    MagicaWindZone magicaWindZone;
    [SerializeField, Header("加算する風力の値")]
    private float m_SetWindPower;
    //初期の風力の保存
    private float m_StertWindPower;

    private void Start()
    {
        if (magicaWindZone != null)
            m_StertWindPower = magicaWindZone.main;
    }
    public void AddWindPower()
    {
        if (magicaWindZone != null)
            magicaWindZone.main = m_SetWindPower;
    }
    public void ResetWindPower()
    {
        if (magicaWindZone != null)
            magicaWindZone.main = m_StertWindPower;
    }
}
