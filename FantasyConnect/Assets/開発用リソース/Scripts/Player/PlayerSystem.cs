using TMPro;
using UnityEngine;
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
    private eState e_CurrentState;
    [Foldout("GetCommponent")]
    [SerializeField]
    public Rigidbody rb;
    [SerializeField]
    private Animator m_PlayerAnimator;
    [SerializeField]
    private Camera mainCamera;
    [Foldout("HPCanvas")]
    [SerializeField,Header("HP�\���p�X���C�_�[")]
    private Slider m_HpSlider;
    [SerializeField, Header("HP�\���p�e�L�X�g")]
    private TextMeshProUGUI m_HpText;
    #region �p�����[�^
    [Foldout("�p�����[�^"), Tab("�p�����[�^")]
    [SerializeField, Header("�ő�Hp")]
    public int m_MaxHp;
    [SerializeField, Header("���݂�Hp")]
    public int m_CurrentHp;
    [SerializeField, Header("�ߐڍU����")]
    private int m_MeleeAttack;
    [SerializeField, Header("���@�U����")]
    private int m_MagicAttack;
    [SerializeField, Header("�ŏ����x")]
    private float m_MinSpeed = 2f;
    [SerializeField, Header("�ő呬�x")]
    public float m_MaxSpeed = 5f;
    [SerializeField, Header("�ʏ�������x")]
    private float m_WalkSpeed = 5;
    [SerializeField, Header("���鑬�x")]
    private float m_RunSpeed = 10;
    [SerializeField, Header("�W�����v�̗�")]
    private float m_JumpForce = 10f;
    [SerializeField, Header("�d��")]
    private float m_Gravity = 9.81f;
    #endregion
    #region�@�g���K�[
    [Foldout("�g���K�[")]
    private bool isGrounded = true;
    private bool isMoving;
    private bool isAttck = false;
    public bool isDie = false;
    #endregion
    #region �T�E���h
    [Foldout("Clip"),Tab("Clip")]
    [SerializeField, Header("��e�{�C�X")]
    private AudioClip m_HitVoice;
    [SerializeField, Header("���S�{�C�X")]
    private AudioClip m_DieVoice;
    [SerializeField, Header("��eSE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("���SSE")]
    private AudioClip m_DieSEClip;
    [Foldout("�I�[�f�B�I�\�[�X")]
    [SerializeField, Header("�I�[�f�B�I�{�C�X")]
    private AudioSource m_Voice;
    [SerializeField, Header("SE")]
    private AudioSource m_SE;
    #endregion
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        SetHp();
    }


    void Update()
    {

        if (isDie)
        return;
        MovePlayer();
        ApplyGravity();
        Die();
      
    }
    void SetHp()
    {
        //Slider�𖞃^���ɂ���B
        m_HpSlider.value = 1;
        //���݂�HP���ő�HP�Ɠ����ɁB
        m_CurrentHp = m_MaxHp;
        // m_HpText �̏�����
        m_HpText.text = m_CurrentHp + "/" + m_MaxHp;
    }
    void MovePlayer()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        m_PlayerAnimator.SetFloat("���E", Input.GetAxis("Horizontal"));
        m_PlayerAnimator.SetFloat("�O��", Input.GetAxis("Vertical"));
        if (m_PlayerAnimator.GetBool("MagicAttck"))
        {
            m_MaxSpeed = 0;
        }

        // �L�[���͂����邩�ǂ������`�F�b�N
        isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

        if (isMoving && isGrounded)
        {
            m_PlayerAnimator.SetBool("Idle", false);
            m_PlayerAnimator.SetBool("Walk", true);
            // �v���C���[�̃��[�J�����W�ł̈ړ��x�N�g�����v�Z
            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            // ���x���ŏ��l����ő�l�͈͓̔��ɐ���
            Vector3 moveVelocity = moveDirection * Mathf.Lerp(m_MinSpeed, m_MaxSpeed, Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));

            // �v���C���[�����[�J�����W�ňړ�
            rb.velocity = moveVelocity;

            // �J�����̕������擾���ăv���C���[�I�u�W�F�N�g����]������
            RotatePlayerWithCamera();
            Run();

        }
        else
        {
            Jump();
            m_PlayerAnimator.SetBool("Idle", true);
            m_PlayerAnimator.SetBool("Walk", false);
            m_PlayerAnimator.SetBool("Run", false);
            // �L�[���͂��Ȃ��ꍇ�A���x��0�ɐݒ�
            rb.velocity = Vector3.zero;
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
        // ���݂̑��x�ɏd�͂�������
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
            m_PlayerAnimator.SetFloat("���荶�E", Input.GetAxis("Horizontal"));
            m_PlayerAnimator.SetFloat("����O��", Input.GetAxis("Vertical"));
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
        //HP�o�[�̍X�V
        m_HpSlider.value = (float)m_CurrentHp / (float)m_MaxHp;
        //HP�e�L�X�g�̍X�V
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
