using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCSpeakSystem : MonoBehaviour
{
    private Transform player;
    [SerializeField,Header("UI�\���p�̋���")]
    public float m_TriggerDistance = 10f;
    [SerializeField,Header("�\������UI")]
    public GameObject m_SpeakUI; 
    [SerializeField]
    public TextTrigger textTrigger;

    private void Start()
    {
        textTrigger=this.GetComponent<TextTrigger>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= m_TriggerDistance)
        {
            // �v���C���[���w��̋����ȓ��ɂ���ꍇ�AUI��\������
            m_SpeakUI.SetActive(true);
            Vector3 UIPos = transform.position;
            UIPos.y += 2;
            m_SpeakUI.transform.position = UIPos;
            if (Input.GetKeyDown(KeyCode.T))
            {
                // T�L�[�������ꂽ��TextTrigger�̃��\�b�h���Ăяo���ăe�L�X�g��\������
                textTrigger.TriggerTextDisplay();
            }
          
        }
        else
        {
            // �v���C���[���w��̋����O�ɂ���ꍇ�AUI���\���ɂ���
            m_SpeakUI.SetActive(false);
            textTrigger.ResetTextIndex(); // �v���C���[�����ꂽ��e�L�X�g�̃C���f�b�N�X�����Z�b�g
        }
    
    }
}
