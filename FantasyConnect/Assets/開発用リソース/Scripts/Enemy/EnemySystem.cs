using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemySystem : MonoBehaviour
{
    #region タイプ
    enum EnemyType
    {
        Enemy,
        NPC,
        Animal,
    }
    enum ButtleType
    {
        Melee,
        Magic,
    }
    [SerializeField, Header("戦闘タイプ")]
    private ButtleType myButtleType;
    [SerializeField, Header("タイプ")]
    private EnemyType myType;
    #endregion
    #region レイキャスト
    [SerializeField, Header("レイキャスト距離")]
    private float m_RaycastDistance = 10f;
    [SerializeField, Header("レイキャスト角度")]
    private float m_Angle = 90f;
    #endregion
    #region 行動パラメーター
    [SerializeField, Header("追跡対象のタグ")]
    private string m_TargetTag = "Player";
    [SerializeField, Header("最大速度")]
    private float m_MaxSpeed = 5f;
    [SerializeField, Header("加速値")]
    private float m_Acceleration = 2f;
    [SerializeField, Header("攻撃距離")]
    private float attackDistance = 5f;
    [SerializeField, Header("回転速度")]
    private float m_RotationSpeed = 5f;
    [SerializeField, Header("魔法攻撃クールタイム")]
    public float m_MagicAttckCoolTime;
    [SerializeField, Header("近接アタッククールタイム")]
    private float m_MeleeCoolTime;
    #endregion

    private Transform player;

    [SerializeField]
    public int m_MaxHp;
    [SerializeField]
    public int m_CurrentHp;

    #region 所得コンポーネント
    [SerializeField, Header("アニメ-ター")]
    private Animator m_Animator;
    private Rigidbody rb;
    #endregion
    #region 各種トリガー
    private bool inAttackRange = false;
    public bool isAttacking = false;
    private bool isMoving = false;
    private bool isDie = false;
    #endregion

    [SerializeField, Header("死亡時エフェクト")]
    private GameObject m_DieEffect;
    [SerializeField]
    private GameObject m_HpBer;
    [SerializeField, Header("Hpバーの位置")]
    private Vector3 m_HpBerPos;
    [SerializeField, Header("Hpバーのサイズ")]
    private Vector3 m_HpBerscale;
    private Vector3 currentVelocity;
    EnemyStatus enemyStatus;
    void Start()
    {
        // プレイヤーオブジェクトのTransformを取得
        player = GameObject.FindGameObjectWithTag(m_TargetTag).transform;
        rb = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        SetHpBer();
    }
    void SetHpBer()
    {
        m_CurrentHp = m_MaxHp;
        GameObject cildrenHpBer = Instantiate(m_HpBer, transform);
        cildrenHpBer.transform.localPosition = m_HpBerPos;
        cildrenHpBer.transform.localScale = m_HpBerscale;
    }

    void Update()
    {
        if (isDie)
            return;
        Search();
        UpdateAnimation();

        if (rb.velocity.magnitude > 0.1f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        Die();
    }
    #region プレイヤー追跡処理
    void Search()
    {
        // レイキャストの発射とターゲットの検知
        int rayCount = 180;
        float angleStep = m_Angle / (rayCount - 1);

        for (int i = 0; i < rayCount; i++)
        {
            float rayAngle = -m_Angle / 2 + angleStep * i;
            Vector3 rayDirection = Quaternion.Euler(0, rayAngle, 0) * transform.forward;
            rayDirection.y = 0;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, m_RaycastDistance))
            {
                // ターゲットに当たった場合
                if (hit.collider.CompareTag(m_TargetTag))
                {
                    PlayerTracking();
                    if (myType == EnemyType.Enemy)
                    {
                        if (myButtleType == ButtleType.Melee)
                        {
                            PlayerAttckDictance();
                        }
                        else if (myButtleType == ButtleType.Magic)
                        {
                            PlayerMagicAttack();
                        }
                    }
                }

                //ヒットしてたら赤に
                Debug.DrawRay(transform.position, rayDirection * hit.distance, Color.red);
            }
            else
            {
                //ヒットしてなければ青色にする
                Debug.DrawRay(transform.position, rayDirection * m_RaycastDistance, Color.blue);
            }
        }
    }

    void PlayerTracking()
    {
        // プレイヤーの方向を向く
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f; // y軸方向は無視して水平に向くようにする
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSpeed);

        // プレイヤーオブジェクトに向かって一定の速度で加速
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 desiredVelocity = direction * m_MaxSpeed;

        // 一定の速度で移動するために、速度を直接設定する
        rb.velocity = desiredVelocity;
    }
    #endregion
    #region プレイヤー攻撃処理
    void PlayerMagicAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 攻撃距離内に入ったら攻撃準備
        if (distanceToPlayer <= attackDistance)
        {
            m_MagicAttckCoolTime += Time.deltaTime;
            if (m_MagicAttckCoolTime > 5)
            {
                inAttackRange = true;
                if (!isAttacking)
                {
                    isAttacking = true;
                    // マジック攻撃のアニメーションを再生
                    m_Animator.SetBool("MagicAttack", true);

                    EnemyMagicWeapon magicWeapon = GetComponentInChildren<EnemyMagicWeapon>();
                    if (magicWeapon != null)
                    {
                        magicWeapon.isMagicAttck = true;
                    }
                }
            }
        }
        else
        {
            inAttackRange = false;
        }


        // 移動中かつ攻撃距離外にいる場合に移動を行う
        if (!isAttacking && rb.velocity.magnitude > 0.1f && !inAttackRange)
        {
            PlayerTracking();
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }
    void PlayerAttckDictance()
    {
        // プレイヤーとの距離を測定
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 攻撃距離内に入ったら攻撃準備
        if (distanceToPlayer <= attackDistance)
        {
            inAttackRange = true;
            if (!isAttacking)
            {
                isAttacking = true;
                StartCoroutine(Attack());
            }
        }
        else
        {
            inAttackRange = false;
        }


        // 移動中かつ攻撃距離外にいる場合に移動を行う
        if (!isAttacking && rb.velocity.magnitude > 0.1f && !inAttackRange)
        {
            PlayerTracking();
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    IEnumerator Attack()
    {
        EnemyAttckCol enemyAttckCol = GetComponentInChildren<EnemyAttckCol>();
        enemyAttckCol.isAttack = true;
        // 攻撃アニメーションを再生
        m_Animator.SetBool("Attack", true);

        // 攻撃後、一定の時間待つ
        yield return new WaitForSeconds(m_MeleeCoolTime);

        // 攻撃が終了したことを示すフラグをリセット
        isAttacking = false;
        m_Animator.SetBool("Attack", false);
        enemyAttckCol.isAttack = false;

    }
    #endregion
    void UpdateAnimation()
    {
        // isMoving の状態に基づいてアニメーションを変更
        m_Animator.SetBool("IsMoving", isMoving);
    }
    public void TakeDamage(int damage)
    {
        if (m_CurrentHp > 0)
        {
            m_CurrentHp -= damage;
        }
    }
    void Die()
    {
        if (m_CurrentHp <= 0)
        {
            m_Animator.SetBool("Die", true);
            m_MaxSpeed = 0;
            isDie = true;
        }
    }
    void DieEnd()
    {
        Instantiate(m_DieEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

