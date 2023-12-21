using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCSpeakSystem : MonoBehaviour
{
    private Transform player;
    [SerializeField,Header("UI表示用の距離")]
    public float m_TriggerDistance = 10f;
    [SerializeField,Header("表示するUI")]
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
            // プレイヤーが指定の距離以内にいる場合、UIを表示する
            m_SpeakUI.SetActive(true);
            Vector3 UIPos = transform.position;
            UIPos.y += 2;
            m_SpeakUI.transform.position = UIPos;
            if (Input.GetKeyDown(KeyCode.T))
            {
                // Tキーが押されたらTextTriggerのメソッドを呼び出してテキストを表示する
                textTrigger.TriggerTextDisplay();
            }
          
        }
        else
        {
            // プレイヤーが指定の距離外にいる場合、UIを非表示にする
            m_SpeakUI.SetActive(false);
            textTrigger.ResetTextIndex(); // プレイヤーが離れたらテキストのインデックスをリセット
        }
    
    }
}
