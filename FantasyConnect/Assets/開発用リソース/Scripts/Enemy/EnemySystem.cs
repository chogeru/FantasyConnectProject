using System.Collections;
using UnityEngine;
using VInspector;
using TMPro;
using System.Collections.Generic;
using MalbersAnimations;

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

    [Foldout("パラメータ"), Tab("パラメータ")]
    #region 行動パラメーター
    
    [SerializeField, Header("最大速度")]
    private float m_MaxSpeed = 5f;
    [SerializeField, Header("現在の速度")]
    private float m_CurrentSpeed = 5f;

    [SerializeField, Header("魔法攻撃クールタイム")]
    public float m_MagicAttckCoolTime;
    [SerializeField, Header("近接アタッククールタイム")]
    private float m_MeleeCoolTime;
 

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

    [Foldout("ステータスデータ"),Tab("ステータスデータ")]
    [SerializeField]
    private EnemyData enemyData;
    public EnemyData EnemyData
    {
        get { return enemyData; }
    }
    #region サウンド
    [Tab("サウンド")]
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
        player = GameObject.FindGameObjectWithTag(enemyData.TargetTag).transform;
        rb = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        SetHpBer();
    }
    void SetHpBer()
    {
        m_CurrentHp = enemyData.MaxHp;
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
        ApplyGravity();

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
                m_SE.clip = enemyData.FootStepClip;
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
        if (distanceToPlayer <= enemyData.RaycastDistance)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f;
            if (angleToPlayer <= 90f)
            {

                RaycastHit hit;
                if (Physics.Raycast(raycastOrigin, directionToPlayer, out hit, enemyData.RaycastDistance, obstacleMask))
                {
                    // ターゲットに当たった場合
                    if (hit.collider.CompareTag(enemyData.TargetTag))
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
                    Debug.DrawRay(raycastOrigin, directionToPlayer * enemyData.RaycastDistance, Color.blue);
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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * enemyData.RotationSpeed);

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
        if (distanceToPlayer <= enemyData.AttackDistance)
        {
            m_MagicAttckCoolTime += Time.deltaTime;
            if (m_MagicAttckCoolTime > 2)
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
    void ApplyGravity()
    {
        rb.AddForce(Vector3.down * enemyData.Gravity, ForceMode.Acceleration);
    }
    void PlayerAttckDictance()
    {
        // プレイヤーとの距離を測定
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 攻撃距離内に入ったら攻撃準備
        if (distanceToPlayer <=  enemyData.AttackDistance)
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
        m_Animator.SetBool("IsMoving", isMoving);
    }
    private void DropItems()
    {
        foreach (var itemInfo in dropItemsInfo)
        {
            if (Random.value <= itemInfo.dropChance)
            {
                int dropCount = Random.Range(0, 8);

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
                m_ViceSE.clip = enemyData.HitVoiceClip;
                m_ViceSE.Play();
                m_SE.clip = enemyData.HitSEClip;
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
            m_ViceSE.clip = enemyData.DieVoiceClip;
            m_ViceSE.Play();
            m_SE.clip = enemyData.DieSEClip;
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
        
        currencySystem.m_currencyAmount += enemyData.DropMony;
        currencySystem.UpdateCurrencyText();
        currencySystem.SaveCurrencyToJson();
        // アイテムをドロップ
        DropItems();
        GameObject effect = EffectObjectPool.Instance.GetPooledObject();
        effect.transform.position = transform.position;
        effect.transform.rotation = Quaternion.identity;
        effect.SetActive(true);
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

