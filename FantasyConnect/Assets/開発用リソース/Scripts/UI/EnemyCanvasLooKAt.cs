using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EnemyCanvasLooKAt : MonoBehaviour
{
    private Transform m_Player;
    EnemySystem enemySystem;
    [SerializeField]
    private Slider m_HpSlider;
    [SerializeField, Header("HP�\���p�e�L�X�g")]
    private TextMeshProUGUI m_HpText;
    void Start()
    {
        m_Player = GameObject.FindGameObjectWithTag("Player").transform;
        enemySystem=GetComponentInParent<EnemySystem>();
        SetHp();
    }
    void SetHp()
    {
        //Slider�𖞃^���ɂ���B
        m_HpSlider.value = 1;
        //���݂�HP���ő�HP�Ɠ����ɁB
        enemySystem.m_CurrentHp = enemySystem.m_MaxHp;
        // m_HpText �̏�����
        m_HpText.text = enemySystem.m_CurrentHp + "/" + enemySystem.m_MaxHp;
    }

    // Update is called once per frame
    void Update()
    {
      transform.LookAt(m_Player);  
        HpUpdate();
    }
    public void HpUpdate()
    {
        // HP��0�ȉ��ɂȂ�Ȃ��悤�ɐ��䂷��
        enemySystem.m_CurrentHp = Mathf.Max(0, enemySystem.m_CurrentHp);

        // HP�o�[�̍X�V
        m_HpSlider.value = (float)enemySystem.m_CurrentHp / (float)enemySystem.m_MaxHp;

        // HP�e�L�X�g�̍X�V
        m_HpText.text = enemySystem.m_CurrentHp + "/" + enemySystem.m_MaxHp;
    }
}
