using TMPro;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VInspector;
public class PlayerSystem : MonoBehaviour
{
    public VInspectorData vInspector;
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
    enum eAttckType
    {
        NomalAttck,
        StrongAttack,
    }
    private eAttckType attckType;

    [SerializeField]
    private PlayerType playerType; 

    private eState e_CurrentState;

    [SerializeField]
    private ParticleSystem m_TypeChangeEffect;
    [Foldout("GetCommponent")]
    [SerializeField]
    public Rigidbody rb;
    [SerializeField]
    private Animator m_PlayerAnimator;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    PlayerIconAnime playerIconAnime;
    [Foldout("HPCanvas")]
    [SerializeField,Header("HP表示用スライダー")]
    private Slider m_HpSlider;
    [SerializeField, Header("HP表示用テキスト")]
    private TextMeshProUGUI m_HpText;
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
    [EndFoldout]
    #endregion
    #region　トリガー
    [Foldout("トリガー")]
    private bool isGrounded = true;
    private bool isMoving;
    public bool isAttck = false;
    public bool isStrongAttck = false;
    public bool isWeponChange = true;
    public bool isStop = false;
    public bool isDie = false;
    [EndFoldout]
    #endregion
    #region サウンド
    [Foldout("Clip"),Tab("Clip")]
    [SerializeField, Header("被弾ボイス")]
    private AudioClip m_HitVoice;
    [SerializeField, Header("死亡ボイス")]
    private AudioClip m_DieVoice;
    [SerializeField, Header("被弾SE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("死亡SE")]
    private AudioClip m_DieSEClip;
    [Foldout("オーディオソース")]
    [SerializeField, Header("オーディオボイス")]
    private AudioSource m_Voice;
    [SerializeField, Header("SE")]
    private AudioSource m_SE;
    [EndFoldout]
    [Foldout("アニメーションコントローラー"),Tab("アニメーションコントローラー")]
    [SerializeField] 
    private RuntimeAnimatorController bowAnimatorController;
    [SerializeField] 
    private RuntimeAnimatorController magicAnimatorController;

    [Foldout("武器"), Tab("武器")]
    [SerializeField]
    private GameObject m_MagicWepon;
    [SerializeField]
    private GameObject m_BowWepon;

    #endregion
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        SetHp();
        SwitchAnimator();
    }


    void Update()
    {
        if(isStop)
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
            rb.velocity = Vector3.zero;
            return;
        }
        MovePlayer();
        ApplyGravity();
        WeponTypeChange();
        PlayerTypeChange();
        Die();
      if(Input.GetKeyDown(KeyCode.Tab))
        {
            SceneController.SceneConinstance.isHitCol = true;
        }
      if(Input.GetMouseButton(0))
        {
            m_MaxSpeed = 0;
            isWeponChange = false;
            switch (attckType)
            {
                case eAttckType.NomalAttck:
                    m_PlayerAnimator.SetBool("NormalAttack", true);
                    break;
                case eAttckType.StrongAttack:
                    m_PlayerAnimator.SetBool("StrongAttack", true);
                    break;
                default:

                    break;
            }
        }
    }
    void SetHp()
    {
        //Sliderを満タンにする。
        m_HpSlider.value = 1;
        //現在のHPを最大HPと同じに。
        m_CurrentHp = m_MaxHp;
        // m_HpText の初期化
        m_HpText.text = m_CurrentHp + "/" + m_MaxHp;
    }
    void PlayerTypeChange()
    {
        if(Input.GetKeyDown (KeyCode.T))
        {
            m_TypeChangeEffect.Play();
            switch (playerType)
            {
                case PlayerType.Magic:
                    playerType = PlayerType.Bow;
                    m_BowWepon.SetActive(true);
                    m_MagicWepon.SetActive(false);
                    break;
                case PlayerType.Bow:
                    playerType = PlayerType.Melee;
                    m_BowWepon.SetActive(false);
                    m_MagicWepon.SetActive(false);
                    break;
                case PlayerType.Melee:
                    playerType = PlayerType.Magic;
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
        isAttck = true;
    }
    void StrongAttack()
    {
        isStrongAttck = true;
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
            RotatePlayerWithCamera();
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
            if (Input.GetKey(KeyCode.Alpha1))
            {
                attckType = eAttckType.NomalAttck;
            }
            if (Input.GetKey(KeyCode.Alpha2))
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
        // 現在の速度に重力を加える
        rb.AddForce(Vector3.down * m_Gravity, ForceMode.Acceleration);
    }
    void Run()
    {
        if (Input.GetKey(KeyCode.LeftShift) && isMoving && isGrounded)
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
            if (m_CurrentHp < 0)
            {
                m_CurrentHp = 0;
            }

            HpUpdate();
            HitSound();
        }
    }
    private void Die()
    {
        if (m_CurrentHp <= 0)
        {
            DieSound();
            m_PlayerAnimator.SetBool("Die", true);
            isDie = true;
        }
    }
    public void HpUpdate()
    {
        //HPバーの更新
        m_HpSlider.value = (float)m_CurrentHp / (float)m_MaxHp;
        //HPテキストの更新
        m_HpText.text = m_CurrentHp + "/" + m_MaxHp;
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
        m_Voice.clip=m_DieVoice;
        m_Voice.Play();
        m_SE.clip=m_DieSEClip;
        m_SE.Play();
    }
}
