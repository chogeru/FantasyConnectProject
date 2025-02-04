using MalbersAnimations;
using TMPro;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VInspector;
using UnityEngine.InputSystem;
using System.Collections;
using static RootMotion.FinalIK.RagdollUtility;
using MagicaCloth2;
public class PlayerSystem : MonoBehaviour
{
    public VInspectorData vInspector;
    WindController windController;
    //InputSystem
    public MainController mainController;
    [SerializeField]
    private InputActionReference LeftHold;
    [SerializeField]
    private InputActionReference RightHold;
    [SerializeField]
    private InputActionReference RunHold;

    public bool isAttacking = false;
    enum eState
    {
        Idle,
        Walk,
        Run,
        Attck,
        Die,
    }
    public enum PlayerType
    {
        Magic,
        Bow,
        Melee,
    }
    public enum eAttckType
    {
        NomalAttck,
        StrongAttack,
    }


    public eAttckType attckType;

    [SerializeField]
    public PlayerType playerType;

    private eState e_CurrentState;

    [SerializeField]
    private ParticleSystem m_TypeChangeEffect;
    [Foldout("GetCommponent")]
    [SerializeField]
    public Rigidbody rb;
    [SerializeField]
    public Animator m_PlayerAnimator;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    PlayerIconAnime playerIconAnime;
    [SerializeField]
    private AnimalRideSystem animalRideSystem;
    [SerializeField]
    PlayerCameraController playerCameraController;
    [SerializeField]
    InventorySystem inventorySystem;
    [Foldout("パラメータCanvas")]
    [SerializeField, Header("HP表示用スライダー")]
    private Slider m_HpSlider;
    [SerializeField, Header("HP表示用テキスト")]
    private TextMeshProUGUI m_HpText;
    [SerializeField, Header("MP表示用スライダー")]
    private Slider m_MpSlider;
    [SerializeField, Header("MP表示用テキスト")]
    private TextMeshProUGUI m_MpText;

    #region パラメータ
    [Foldout("パラメータ"), Tab("パラメータ")]
    [SerializeField, Header("最大Hp")]
    public int m_MaxHp;
    [SerializeField, Header("現在のHp")]
    public int m_CurrentHp;
    [SerializeField, Header("近接攻撃力")]
    private int m_MeleeAttack;
    [SerializeField, Header("魔法攻撃力")]
    private int m_MagicAttack;
    [SerializeField, Header("最小速度")]
    private float m_MinSpeed = 2f;
    [SerializeField, Header("最大速度")]
    public float m_MaxSpeed = 5f;
    [SerializeField, Header("通常歩く速度")]
    private float m_WalkSpeed = 5;
    [SerializeField, Header("走る速度")]
    private float m_RunSpeed = 10;
    [SerializeField, Header("ジャンプの力")]
    private float m_JumpForce = 10f;
    [SerializeField, Header("重力")]
    private float m_Gravity = 9.81f;
    [SerializeField, Header("MP")]
    public int m_MP = 100;
    public int m_MaxMP = 100;
    float mpRecoveryTimer = 0f; // MP回復用のタイマー
    float mpRecoveryInterval = 1f; // MP回復間隔
    [SerializeField, Header("ヒットストップの時間")]
    public float m_HitStopTime=0f;
    [EndFoldout]
    #endregion
    #region　トリガー
    [Foldout("トリガー")]
    private bool isGrounded = true;
    private bool isMoving;
    private bool isRun = false;
    public bool isAttck = false;
    public bool isStrongAttck = false;
    public bool isMeleeAttckColEnd = false;
    public bool isWeponChange = true;
    public bool isEndAttck = false;
    public bool isStop = false;
    public bool isDie = false;
    public static bool isHitStop = false;
    [EndFoldout]
    #endregion
    #region サウンド
    [Foldout("Clip"), Tab("Clip")]
    [SerializeField, Header("被弾ボイス")]
    private AudioClip m_HitVoice;
    [SerializeField, Header("死亡ボイス")]
    private AudioClip m_DieVoice;
    [SerializeField, Header("被弾SE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("死亡SE")]
    private AudioClip m_DieSEClip;
    [SerializeField, Header("武器チェンジSE")]
    private AudioClip m_AttackChangeSE;
    [SerializeField, Header("アイテム所得時のSE")]
    private AudioClip m_ItemHitSE;
    [Foldout("オーディオソース")]
    [SerializeField, Header("オーディオボイス")]
    private AudioSource m_Voice;
    [SerializeField, Header("SE")]
    private AudioSource m_SE;
    [EndFoldout]
    [Foldout("アニメーションコントローラー"), Tab("アニメーションコントローラー")]
    [SerializeField]
    private RuntimeAnimatorController bowAnimatorController;
    [SerializeField]
    private RuntimeAnimatorController magicAnimatorController;
    [SerializeField]
    private RuntimeAnimatorController meleeAnimatorController;

    [Foldout("武器"), Tab("武器")]
    [SerializeField]
    private GameObject m_MagicWepon;
    [SerializeField]
    private GameObject m_BowWepon;
    [SerializeField]
    private GameObject m_MeleeWepon;

    #endregion
    void Start()
    {
        SetInput();
        mainController = new MainController();
        mainController.Enable();
        playerCameraController.enabled = true;
        rb = GetComponent<Rigidbody>();
        windController = GetComponent<WindController>();
        mainCamera = Camera.main;
        SetParametar();
        SwitchAnimator();
    }

    void Update()
    {
        if (PlayerCanvasButton.isPaused||isHitStop)
        {
            m_PlayerAnimator.speed = 0;
            rb.velocity = Vector3.zero;
            if (isHitStop)
            {
                StartCoroutine(SetHitStop(m_HitStopTime));
            }
            return;
        }
        else
        {
            m_PlayerAnimator.speed = 1;
        }
        if (animalRideSystem.isRide)
        {
            return;
        }
        if (isStop)
        {
            m_PlayerAnimator.SetBool("Idle", true);
            m_PlayerAnimator.SetBool("Walk", false);
            m_PlayerAnimator.SetBool("Run", false);
            rb.velocity = Vector3.zero;

            return;
        }
        else
        {
            m_PlayerAnimator.SetBool("Idle", false);
        }
        if (isDie)
        {
            isMeleeAttckColEnd = true;
            rb.velocity = Vector3.zero;
            return;
        }
       
        RecoverMP();
        MovePlayer();
        ApplyGravity();
        WeponTypeChange();
        WindPowerSet();
        PlayerTypeChange();
        Die();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SceneController.SceneConinstance.isHitCol = true;
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isRun = true;
        }
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || isAttacking)
        {
            m_MaxSpeed = 0;
            m_PlayerAnimator.SetBool("Walk", false);
            m_PlayerAnimator.SetBool("Run", false);
            m_PlayerAnimator.SetBool("Hit", false);

            isWeponChange = false;
            switch (attckType)
            {
                case eAttckType.NomalAttck:
                    if (playerType == PlayerType.Magic && m_MP >= 5 ||
                  playerType == PlayerType.Melee ||
                  (playerType == PlayerType.Bow && (inventorySystem.inventory.ContainsKey("Arrow") && inventorySystem.inventory["Arrow"].amount > 0)))
                    {
                        m_PlayerAnimator.SetBool("NormalAttack", true);
                    }

                    if (playerType == PlayerType.Magic && m_MP <= 5 ||
                         playerType == PlayerType.Bow && !inventorySystem.inventory.ContainsKey("Arrow"))
                    {
                        m_PlayerAnimator.SetBool("NormalAttack", false);
                    }

                    break;
                case eAttckType.StrongAttack:
                    if (playerType == PlayerType.Magic && m_MP >= 10 ||
                        playerType == PlayerType.Melee ||
                        playerType == PlayerType.Bow && inventorySystem.inventory["Arrow"].amount >= 3)
                    {
                        m_PlayerAnimator.SetBool("StrongAttack", true);
                    }
                    if (playerType == PlayerType.Magic && m_MP <= 10 ||
                        playerType == PlayerType.Bow && inventorySystem.inventory["Arrow"].amount < 3)
                    {
                        m_PlayerAnimator.SetBool("StrongAttack", false);
                    }
                    break;
                default:

                    break;
            }
        }
        if (m_PlayerAnimator.GetBool("Hit"))
        {
            m_MaxSpeed = 0;
        }
    }
    private void SetInput()
    {
        if (LeftHold != null)
        {
            LeftHold.action.performed += OnAttckHold;
            LeftHold.action.started += SetLeftWepon;
            LeftHold.action.canceled += OnAttckHoldEnd;
        }
        if (RightHold != null)
        {
            RightHold.action.performed += OnAttckHold;
            RightHold.action.started += SetRightWepon;
            RightHold.action.canceled += OnAttckHoldEnd;
        }
        if (RunHold != null)
        {
            RunHold.action.performed += OnRunHold;
            RunHold.action.canceled += OnRunHoldEnd;
        }
    }
    private void WindPowerSet()
    {
        if (m_PlayerAnimator.GetBool("NormalAttack") || m_PlayerAnimator.GetBool("StrongAttack"))
        {
            if (windController != null)
            {
                windController.AddWindPower();
            }
        }

        else
        {
            if (windController != null)
            {
                windController.ResetWindPower();
            }
        }
    }
    private void FixedUpdate()
    {
        if (isMoving && isGrounded)
        {
            RotatePlayerWithCamera();
        }
    }
    public void SetParametar()
    {
        m_CurrentHp = m_MaxHp;
        m_MP = m_MaxMP;
        HpUpdate();
        MpUpdate();
        m_MP = Mathf.Min(m_MP, m_MaxMP);

    }
    void RecoverMP()
    {
        if (m_MP < 100)
        {
            mpRecoveryTimer += Time.deltaTime;
            if (mpRecoveryTimer >= mpRecoveryInterval)
            {
                mpRecoveryTimer -= mpRecoveryInterval;
                m_MP += 1;
                MpUpdate();
            }
        }
    }
    void PlayerTypeChange()
    {
        if (Input.GetKeyDown(KeyCode.T) || mainController.Player.WeponChange.triggered)
        {
            m_TypeChangeEffect.Play();
            m_SE.clip = m_AttackChangeSE;
            m_SE.Play();
            MagicWepon magicWepon = GetComponentInChildren<MagicWepon>();
            if (magicWepon != null)
            {
                magicWepon.AttackEnd();
            }
            m_PlayerAnimator.SetBool("NormalAttack", false);
            m_PlayerAnimator.SetBool("StrongAttack", false);
            switch (playerType)
            {
                case PlayerType.Magic:
                    playerType = PlayerType.Bow;
                    m_MeleeWepon.SetActive(false);
                    m_BowWepon.SetActive(true);
                    m_MagicWepon.SetActive(false);
                    break;
                case PlayerType.Bow:
                    playerType = PlayerType.Melee;
                    m_MeleeWepon.SetActive(true);
                    m_BowWepon.SetActive(false);
                    m_MagicWepon.SetActive(false);
                    break;
                case PlayerType.Melee:
                    playerType = PlayerType.Magic;
                    m_MeleeWepon.SetActive(false);
                    m_BowWepon.SetActive(false);
                    m_MagicWepon.SetActive(true);
                    break;
                default:
                    break;
            }
            SwitchAnimator();
        }
    }

    void NomalAttck()
    {
        isMeleeAttckColEnd = false;
        isAttck = true;
        //ヒットストップ時間(数値は初代ストリートファイター参考)
        m_HitStopTime = 0.075f;
    }

    void StrongAttack()
    {
        isMeleeAttckColEnd = false;
        isStrongAttck = true;
        //ヒットストップ時間(数値は初代ストリートファイター参考)
        m_HitStopTime = 0.417f;
    }
    void MeleeAttckColEnd()
    {
        isMeleeAttckColEnd = true;
    }
    void EndAttck()
    {
        isEndAttck = true;
    }

    private void SwitchAnimator()
    {
        switch (playerType)
        {
            case PlayerType.Bow:
                m_PlayerAnimator.runtimeAnimatorController = bowAnimatorController;

                break;
            case PlayerType.Magic:
                m_PlayerAnimator.runtimeAnimatorController = magicAnimatorController;
                break;
            case PlayerType.Melee:
                m_PlayerAnimator.runtimeAnimatorController = meleeAnimatorController;
                break;
            default:
                break;
        }
    }
    void MovePlayer()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        m_PlayerAnimator.SetFloat("左右", Input.GetAxis("Horizontal"));
        m_PlayerAnimator.SetFloat("前後", Input.GetAxis("Vertical"));
        if (m_PlayerAnimator.GetBool("StrongAttack"))
        {
            m_MaxSpeed = 0;
        }

        // キー入力があるかどうかをチェック
        isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

        if (isMoving && isGrounded)
        {
            m_PlayerAnimator.SetBool("Idle", false);
            m_PlayerAnimator.SetBool("Walk", true);
            // プレイヤーのローカル座標での移動ベクトルを計算
            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            // 速度を最小値から最大値の範囲内に制限
            Vector3 moveVelocity = moveDirection * Mathf.Lerp(m_MinSpeed, m_MaxSpeed, Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));

            // プレイヤーをローカル座標で移動
            rb.velocity = moveVelocity;

            // カメラの方向を取得してプレイヤーオブジェクトを回転させる

            Run();

        }
        else
        {
            Jump();
            m_PlayerAnimator.SetBool("Idle", true);
            m_PlayerAnimator.SetBool("Walk", false);
            m_PlayerAnimator.SetBool("Run", false);
            // キー入力がない場合、速度を0に設定
            rb.velocity = Vector3.zero;
        }
    }

    private void WeponTypeChange()
    {
        if (isWeponChange)
        {
            if (Input.GetMouseButtonDown(0))
            {
                attckType = eAttckType.NomalAttck;
            }
            if (Input.GetMouseButtonDown(1))
            {
                attckType = eAttckType.StrongAttack;
            }
        }
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
    void ApplyGravity()
    {
        rb.AddForce(Vector3.down * m_Gravity, ForceMode.Acceleration);
    }
    void Run()
    {

        if (isRun && isMoving && isGrounded)
        {
            m_PlayerAnimator.SetBool("Idle", false);
            m_PlayerAnimator.SetBool("Walk", false);
            m_PlayerAnimator.SetBool("Run", true);
            m_MaxSpeed = m_RunSpeed;
            m_PlayerAnimator.SetFloat("走り左右", Input.GetAxis("Horizontal"));
            m_PlayerAnimator.SetFloat("走り前後", Input.GetAxis("Vertical"));
        }
        else
        {
            m_MaxSpeed = m_WalkSpeed;
            m_PlayerAnimator.SetBool("Idle", false);
            m_PlayerAnimator.SetBool("Run", false);
        }
    }
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            m_PlayerAnimator.SetBool("Jump", true);
            rb.AddForce(Vector3.up * m_JumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
        if (isGrounded == false)
        {
            m_PlayerAnimator.SetBool("Walk", false);
            m_PlayerAnimator.SetBool("Run", false);
            m_PlayerAnimator.SetBool("Jump", true);
            m_MaxSpeed = 0f;

        }
    }
    public void TakeDamage(int damage)
    {
        if (m_CurrentHp > 0)
        {
            m_CurrentHp -= damage;
            playerIconAnime.isHit = true;
            m_PlayerAnimator.SetBool("Hit", true);

            if (m_CurrentHp < 0)
            {
                m_CurrentHp = 0;
            }

            HpUpdate();
            HitSound();
        }
    }
    public void HpRecovery(int hpRecovery)
    {
        m_CurrentHp += hpRecovery;
        m_CurrentHp = Mathf.Min(m_CurrentHp, m_MaxHp);
        ItemHitSound();
        HpUpdate();
    }
    public void MPRecovery(int mpRecovery)
    {
        m_MP += mpRecovery;
        m_MP = Mathf.Min(m_MP, m_MaxMP);
        ItemHitSound();
        MpUpdate();
    }
    public void ItemHitSound()
    {
        m_SE.clip = m_ItemHitSE;
        m_SE.Play();
    }
    private void EndHit()
    {
        m_PlayerAnimator.SetBool("Hit", false);
    }
    private void Die()
    {
        if (m_CurrentHp <= 0)
        {
            DieSound();
            m_PlayerAnimator.SetBool("Die", true);
            m_PlayerAnimator.SetBool("Hit", false);
            isDie = true;
        }
    }
    public void HpUpdate()
    {
        m_HpSlider.value = (float)m_CurrentHp / (float)m_MaxHp;
        m_HpText.text = m_CurrentHp + "/" + m_MaxHp;
    }
    public void MpUpdate()
    {
        m_MpSlider.value = (float)m_MP / (float)m_MaxMP;
        m_MpText.text = m_MP + "/" + m_MaxMP;
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        m_MaxSpeed = m_WalkSpeed;
        m_PlayerAnimator.SetBool("Jump", false);
    }
    private void HitSound()
    {
        m_Voice.clip = m_HitVoice;
        m_Voice.Play();

        m_SE.clip = m_HitSEClip;
        m_SE.Play();
    }
    private void DieSound()
    {
        m_Voice.clip = m_DieVoice;
        m_Voice.Play();
        m_SE.clip = m_DieSEClip;
        m_SE.Play();
    }
    IEnumerator SetHitStop(float delay)
    {
        yield return new WaitForSeconds(delay);
        isHitStop = false;
    }
    private void OnAttckHold(InputAction.CallbackContext context)
    {
        isAttacking = true;
    }
    private void OnAttckHoldEnd(InputAction.CallbackContext context)
    {
        isAttacking = false;
    }
    private void SetLeftWepon(InputAction.CallbackContext context)
    {
        attckType = eAttckType.NomalAttck;
    }
    private void SetRightWepon(InputAction.CallbackContext context)
    {
        attckType = eAttckType.StrongAttack;
    }
    private void OnRunHold(InputAction.CallbackContext context)
    {
        isRun = true;
    }
    private void OnRunHoldEnd(InputAction.CallbackContext context)
    {
        isRun = false;
    }
}
