using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReSpown : MonoBehaviour
{
    //最後に通過したチェックポイントの座標
    [SerializeField]
    private Vector3 m_LastChackPointPosition;
    //最後に通過したチェックポイントの回転
    [SerializeField]
    private Quaternion m_LastChackPointRotation;
    public bool isReSpown = false;
    void Start()
    {
        m_LastChackPointPosition = transform.position;
        m_LastChackPointRotation = transform.rotation;
    }
    private void Update()
    {
        if (isReSpown)
        {
            PlayerSystem playerSystem = GetComponent<PlayerSystem>();
            transform.position = m_LastChackPointPosition;
            transform.rotation = m_LastChackPointRotation;
            isReSpown=false;
            if(playerSystem != null)
            {
                playerSystem.m_CurrentHp = playerSystem.m_MaxHp;
                playerSystem.HpUpdate();
                playerSystem.isDie = false;
            }
        }
    }
    private void ReSpownTrigger()
    {
        isReSpown = true;
        Animator animator = GetComponent<Animator>();
        animator.SetBool("Die", false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CheckPoint"))
        {
            //最後のチェックポイントの位置と回転を戻す
            m_LastChackPointPosition = other.transform.position;
            m_LastChackPointRotation = other.transform.rotation;
        }
    }
}
