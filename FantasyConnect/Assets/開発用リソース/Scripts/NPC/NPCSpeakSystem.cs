using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCSpeakSystem : MonoBehaviour
{
    public enum NPCType
    {
        NPC,
        ShopNPC
    }
    public NPCType npcType;
    [SerializeField]
    private GameObject m_ShopCanvas;
    private Transform player;
    [SerializeField, Header("UI�\���p�̋���")]
    public float m_TriggerDistance = 10f;
    [SerializeField, Header("�\������UI")]
    public GameObject m_SpeakUI;
    private GameObject playerCam;

    [SerializeField]
    public TextTrigger textTrigger;
    private PlayerSystem playerSystem;
    [SerializeField]
    PlayerCameraController playerCameraController;
    private void Start()
    {
        textTrigger = this.GetComponent<TextTrigger>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        playerCameraController = playerCam.GetComponent<PlayerCameraController>();
        playerSystem = player.GetComponent<PlayerSystem>();
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
            if (npcType == NPCType.ShopNPC && TextManager.Instance.isTextEnd)
            {
                Cursor.visible = true;
                playerCameraController.isStop = true;
                playerSystem.isStop = true;
                m_ShopCanvas.SetActive(true);
            }
        }
        else
        {
            // �v���C���[���w��̋����O�ɂ���ꍇ�AUI���\���ɂ���
            m_SpeakUI.SetActive(false);
            textTrigger.ResetTextIndex(); // �v���C���[�����ꂽ��e�L�X�g�̃C���f�b�N�X�����Z�b�g7
            TextManager.Instance.isTextEnd = false;
        }

    }
    public void CanvasClose()
    {
        Cursor.visible = false;
        playerCameraController.isStop = false;
        playerSystem.isStop = false;
        m_ShopCanvas.SetActive(false);
        TextManager.Instance.isTextEnd = false;

    }
}
