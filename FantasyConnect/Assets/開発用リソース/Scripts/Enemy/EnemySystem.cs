using System.Collections;
using UnityEngine;
using VInspector;
public class EnemySystem : MonoBehaviour
{
    public VInspectorData VInspectorData;
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
    [Foldout("レイキャスト"),Tab("レイキャスト")]
    #region レイキャスト
    [SerializeField, Header("レイキャスト距離")]
    private float m_RaycastDistance = 10f;
    [SerializeField, Header("レイキャスト角度")]
    private float m_Angle = 90f;
    [SerializeField, Header("レイの数")]
    private int rayCount = 180;
    #endregion
    [Foldout("パラメータ"),Tab("パラメータ")]
    #region 行動パラメーター
    [SerializeField, Header("追跡対象のタグ")]
    private string m_TargetTag = "Player";
    [SerializeField, Header("最大速度")]
    private float m_MaxSpeed = 5f;
    [SerializeField, Header("現在の速度")]
    private float m_CurrentSpeed = 5f;
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
    [SerializeField]
    public int m_MaxHp;
    [SerializeField]
    public int m_CurrentHp;
    #endregion

    private Transform player;

    [Foldout("コンポーネント")]
    #region 所得コンポーネント
    [SerializeField, Header("アニメ-ター")]
    private Animator m_Animator;
    private Rigidbody rb;
    [EndFoldout]
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
    public LayerMask obstacleMask;
    EnemyStatus enemyStatus;
    #region サウンド
    [Foldout("音声クリップ"), Tab("音声クリップ")]
    [SerializeField, Header("ヒットボイスクリップ")]
    private AudioClip m_HitVoiceClip;
    [SerializeField, Header("死亡時ボイスクリップ")]
    private AudioClip m_DieVoiceClip;
    [SerializeField,Header("被弾SE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("死亡SE")]
    private AudioClip m_DieSEClip;
    [Foldout("オーディオソース")]
    [SerializeField,Header("ボイス")]
    private AudioSource m_ViceSE;
    [SerializeField, Header("SE")]
    private AudioSource m_SE;
#endregion
    [EndFoldout]
    private bool isHit = false;

    void Start()
    {
        m_CurrentSpeed = m_MaxSpeed;
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
        if (isHit)
            return;
        if (isDie)
            return;
        m_MaxSpeed = m_CurrentSpeed;
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
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= 10f)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f;
            if (angleToPlayer <= 90f)
            {
                
                RaycastHit hit;
                if (Physics.Raycast(raycastOrigin, directionToPlayer, out hit, m_RaycastDistance,obstacleMask))
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
                    Debug.DrawRay(raycastOrigin, directionToPlayer * hit.distance, Color.red);
                }
                else
                {
                    //ヒットしてなければ青色にする
                    Debug.DrawRay(raycastOrigin, directionToPlayer * m_RaycastDistance, Color.blue);
                }
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
            m_Animator.SetBool("Hit",true);
            m_MaxSpeed = 0;
            isHit = true;
            m_ViceSE.clip = m_HitVoiceClip;
            m_ViceSE.Play();
            m_SE.clip = m_HitSEClip;
            m_SE.Play();
        }
    }
    private void EndHit()
    {
        m_Animator.SetBool("Hit", false);
        m_MaxSpeed = m_CurrentSpeed;
        isHit=false;
    }
    void Die()
    {
        if (m_CurrentHp <= 0)
        {
            m_Animator.SetBool("Die", true);
            m_ViceSE.clip = m_DieVoiceClip;
            m_ViceSE.Play();
            m_SE.clip = m_DieSEClip;
            m_SE.Play();
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

