using System.Collections;
using UnityEngine;
using VInspector;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DropItemInfo
//アイテムの情報を所持
{
    public GameObject itemPrefab;
    public float dropChance;
}
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
    [Foldout("レイキャスト"), Tab("レイキャスト")]
    #region レイキャスト
    [SerializeField, Header("レイキャスト距離")]
    private float m_RaycastDistance = 10f;
    [SerializeField, Header("レイキャスト角度")]
    private float m_Angle = 90f;
    [SerializeField, Header("レイの数")]
    private int rayCount = 180;

    #endregion
    [Foldout("パラメータ"), Tab("パラメータ")]
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
    [SerializeField, Header("ドロップする金額")]
    private int m_DropMony = 500;
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
    [SerializeField, Header("攻撃トレイル")]
    private GameObject m_AttackEffect;


    [SerializeField]
    private GameObject m_HpBer;
    [SerializeField, Header("Hpバーの位置")]
    private Vector3 m_HpBerPos;
    [SerializeField, Header("Hpバーのサイズ")]
    private Vector3 m_HpBerscale;
    private Vector3 currentVelocity;
    public LayerMask obstacleMask;
    [SerializeField]
    EnemyData enemyData;
    #region サウンド
    [Foldout("音声クリップ"), Tab("音声クリップ")]
    [SerializeField, Header("ヒットボイスクリップ")]
    private AudioClip m_HitVoiceClip;
    [SerializeField, Header("死亡時ボイスクリップ")]
    private AudioClip m_DieVoiceClip;
    [SerializeField, Header("被弾SE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("死亡SE")]
    private AudioClip m_DieSEClip;
    [SerializeField,Header("足音")]
    private AudioClip m_FootStepClip;
    [Foldout("オーディオソース")]
    [SerializeField, Header("ボイス")]
    private AudioSource m_ViceSE;
    [SerializeField, Header("SE")]
    private AudioSource m_SE;
    #endregion
    [EndFoldout]
    private bool isHit = false;
    private bool isRide = false;
    [Tab("ドロップアイテム")]
    [SerializeField, Header("ドロップアイテムリスト")]
    private List<DropItemInfo> dropItemsInfo = new List<DropItemInfo>();

    private CurrencySystem currencySystem;
    private Camera mainCamera;

    void Start()
    {
        m_CurrentSpeed = m_MaxSpeed;
        if (m_AttackEffect != null)
        {
            m_AttackEffect.SetActive(false);
        }
        if (myType == EnemyType.Animal)
        {
            mainCamera = Camera.main;
        }
        if (enemyData == null)
        {
            enemyData = ScriptableObject.CreateInstance<EnemyData>();
        }
        currencySystem = FindObjectOfType<CurrencySystem>();
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
        if (isRide)
        {
            HandleWASDMovement();
        }
        else
        {
            RideEnd();
            if (isHit)
                return;
            if (isDie)
            {
                return;
            }
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
     
    }
    /// <summary>
    /// ライド中の処理
    /// </summary>
    private void HandleWASDMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        m_Animator.SetFloat("左右", Input.GetAxis("Horizontal"));
        m_Animator.SetFloat("前後", Input.GetAxis("Vertical"));
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

        // 速度を最小値から最大値の範囲内に制限
        Vector3 moveVelocity = moveDirection * Mathf.Lerp(0, 15, Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));
        m_Animator.SetBool("Ride", true);
        // プレイヤーをローカル座標で移動
        rb.velocity = moveVelocity;
        if (isMoving)
        {
            // カメラの方向を取得してプレイヤーオブジェクトを回転させる
            RotatePlayerWithCamera();
            if (!m_SE.isPlaying) // 再生中でない場合のみ再生
            {
                m_SE.clip = m_FootStepClip;
                m_SE.Play();
            }
            if(!m_ViceSE.isPlaying)
            {
                m_ViceSE.Play();
            }
        }
        else
        {
         RideEnd();
        }
    
    }
    private void RideEnd()
    {
        m_SE.Stop();
        m_ViceSE.Stop();
        m_Animator.SetBool("Ride", false);
    }
    void RotatePlayerWithCamera()
    {
        if (mainCamera != null)
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0f;

            if (cameraForward != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(cameraForward);
                rb.MoveRotation(newRotation);
            }
        }
    }
    public void SetIsRide(bool value)
    {
        isRide = value;
    }
    /// <summary>
    /// ライド中の処理終了
    /// </summary>
    #region プレイヤー追跡処理
    void Search()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= m_RaycastDistance)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f;
            if (angleToPlayer <= 90f)
            {

                RaycastHit hit;
                if (Physics.Raycast(raycastOrigin, directionToPlayer, out hit, m_RaycastDistance, obstacleMask))
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
        if (m_AttackEffect != null)
        {
            m_AttackEffect.SetActive(true);
        }
        // 攻撃後、一定の時間待つ
        yield return new WaitForSeconds(m_MeleeCoolTime);

        // 攻撃が終了したことを示すフラグをリセット
        isAttacking = false;
        m_Animator.SetBool("Attack", false);
        if (m_AttackEffect != null)
        {
            m_AttackEffect.SetActive(false);
        }
        enemyAttckCol.isAttack = false;

    }
    #endregion
    void UpdateAnimation()
    {
        // isMoving の状態に基づいてアニメーションを変更
        m_Animator.SetBool("IsMoving", isMoving);
    }
    private void DropItems()
    {
        foreach (var itemInfo in dropItemsInfo)
        {
            if (Random.value <= itemInfo.dropChance)
            {
                // アイテムをドロップする回数を決定（ここでは最大3回まで）
                int dropCount = Random.Range(0, 6);

                for (int i = 0; i < dropCount; i++)
                {
                    Instantiate(itemInfo.itemPrefab, transform.position + Random.insideUnitSphere, Quaternion.identity);
                }
            }
        }
    }
    public void TakeDamage(int damage)
    {

        if (m_CurrentHp > 0)
        {
            m_CurrentHp -= damage;
            if (m_CurrentHp > 0)
            {
                m_Animator.SetBool("Hit", true);
                m_MaxSpeed = 0;
                isHit = true;
                m_ViceSE.clip = m_HitVoiceClip;
                m_ViceSE.Play();
                m_SE.clip = m_HitSEClip;
                m_SE.Play();

                // 攻撃を受けたらプレイヤーの方向を向く
                Vector3 lookDirection = player.position - transform.position;
                lookDirection.y = 0f; // y軸方向は無視して水平に向くようにする
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = targetRotation;
            }
        }
    }
    private void EndHit()
    {
        m_Animator.SetBool("Hit", false);
        m_MaxSpeed = m_CurrentSpeed;
        isHit = false;
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
            if (myButtleType == ButtleType.Magic)
            {
                rb.useGravity = true;
            }
        }
    }
    void DieEnd()
    {
        currencySystem.m_currencyAmount += m_DropMony;
        currencySystem.UpdateCurrencyText();
        // アイテムをドロップ
        DropItems();
        Instantiate(m_DieEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && myType == EnemyType.Animal)
        {
            isHit = true;
            m_MaxSpeed = 0;
            isMoving = false;
            UpdateAnimation();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && myType == EnemyType.Animal)
        {
            isHit = false;
            m_MaxSpeed = m_CurrentSpeed;
            UpdateAnimation();
        }
    }
}

