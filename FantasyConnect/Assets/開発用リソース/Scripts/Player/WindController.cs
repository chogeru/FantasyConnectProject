using MagicaCloth2;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindController : MonoBehaviour
{
    [SerializeField, Header("�}�b�v�ɔz�u���Ă���MagicWindZon")]
    MagicaWindZone magicaWindZone;
    [SerializeField, Header("���Z���镗�͂̒l")]
    private float m_SetWindPower;
    //�����̕��͂̕ۑ�
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
