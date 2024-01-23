using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EnemyCanvasLooKAt : MonoBehaviour
{
    private Transform m_Player;
    EnemySystem enemySystem;
    [SerializeField]
    private Slider m_HpSlider;
    [SerializeField, Header("HP表示用テキスト")]
    private TextMeshProUGUI m_HpText;
    EnemyData enemyData;
    void Start()
    {
        m_Player = GameObject.FindGameObjectWithTag("MainCamera").transform;
        enemySystem=GetComponentInParent<EnemySystem>();
        enemyData = enemySystem.EnemyData;
        SetHp();
    }
    void SetHp()
    {
        //Sliderを満タンにする。
        m_HpSlider.value = 1;
        //現在のHPを最大HPと同じに。
        enemySystem.m_CurrentHp = enemySystem.EnemyData.MaxHp;
        // m_HpText の初期化
        m_HpText.text = enemySystem.m_CurrentHp + "/" + enemySystem.EnemyData.MaxHp;
    }

    // Update is called once per frame
    void Update()
    {
      transform.LookAt(m_Player);  
        HpUpdate();
    }
    public void HpUpdate()
    {
        // HPが0以下にならないように制御する
        enemySystem.m_CurrentHp = Mathf.Max(0, enemySystem.m_CurrentHp);

        // HPバーの更新
        m_HpSlider.value = (float)enemySystem.m_CurrentHp / (float)enemySystem.EnemyData.MaxHp;

        // HPテキストの更新
        m_HpText.text = enemySystem.m_CurrentHp + "/" + enemySystem.EnemyData.MaxHp;
    }
}
