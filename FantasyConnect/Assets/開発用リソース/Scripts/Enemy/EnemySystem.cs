using System.Collections;
using UnityEngine;
using VInspector;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DropItemInfo
//�A�C�e���̏�������
{
    public GameObject itemPrefab;
    public float dropChance;
}
public class EnemySystem : MonoBehaviour
{
    public VInspectorData VInspectorData;
    #region �^�C�v
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
    [SerializeField, Header("�퓬�^�C�v")]
    private ButtleType myButtleType;
    [SerializeField, Header("�^�C�v")]
    private EnemyType myType;
    #endregion
    [Foldout("���C�L���X�g"), Tab("���C�L���X�g")]
    #region ���C�L���X�g
    [SerializeField, Header("���C�L���X�g����")]
    private float m_RaycastDistance = 10f;
    [SerializeField, Header("���C�L���X�g�p�x")]
    private float m_Angle = 90f;
    [SerializeField, Header("���C�̐�")]
    private int rayCount = 180;

    #endregion
    [Foldout("�p�����[�^"), Tab("�p�����[�^")]
    #region �s���p�����[�^�[
    [SerializeField, Header("�ǐՑΏۂ̃^�O")]
    private string m_TargetTag = "Player";
    [SerializeField, Header("�ő呬�x")]
    private float m_MaxSpeed = 5f;
    [SerializeField, Header("���݂̑��x")]
    private float m_CurrentSpeed = 5f;
    [SerializeField, Header("�����l")]
    private float m_Acceleration = 2f;
    [SerializeField, Header("�U������")]
    private float attackDistance = 5f;
    [SerializeField, Header("��]���x")]
    private float m_RotationSpeed = 5f;
    [SerializeField, Header("���@�U���N�[���^�C��")]
    public float m_MagicAttckCoolTime;
    [SerializeField, Header("�ߐڃA�^�b�N�N�[���^�C��")]
    private float m_MeleeCoolTime;
    [SerializeField]
    public int m_MaxHp;
    [SerializeField]
    public int m_CurrentHp;
    [SerializeField, Header("�h���b�v������z")]
    private int m_DropMony = 500;
    #endregion

    private Transform player;

    [Foldout("�R���|�[�l���g")]
    #region �����R���|�[�l���g
    [SerializeField, Header("�A�j��-�^�[")]
    private Animator m_Animator;
    private Rigidbody rb;
    [EndFoldout]
    #endregion
    #region �e��g���K�[
    private bool inAttackRange = false;
    public bool isAttacking = false;
    private bool isMoving = false;
    private bool isDie = false;
    #endregion

    [SerializeField, Header("���S���G�t�F�N�g")]
    private GameObject m_DieEffect;
    [SerializeField, Header("�U���g���C��")]
    private GameObject m_AttackEffect;


    [SerializeField]
    private GameObject m_HpBer;
    [SerializeField, Header("Hp�o�[�̈ʒu")]
    private Vector3 m_HpBerPos;
    [SerializeField, Header("Hp�o�[�̃T�C�Y")]
    private Vector3 m_HpBerscale;
    private Vector3 currentVelocity;
    public LayerMask obstacleMask;
    [SerializeField]
    EnemyData enemyData;
    #region �T�E���h
    [Foldout("�����N���b�v"), Tab("�����N���b�v")]
    [SerializeField, Header("�q�b�g�{�C�X�N���b�v")]
    private AudioClip m_HitVoiceClip;
    [SerializeField, Header("���S���{�C�X�N���b�v")]
    private AudioClip m_DieVoiceClip;
    [SerializeField, Header("��eSE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("���SSE")]
    private AudioClip m_DieSEClip;
    [SerializeField,Header("����")]
    private AudioClip m_FootStepClip;
    [Foldout("�I�[�f�B�I�\�[�X")]
    [SerializeField, Header("�{�C�X")]
    private AudioSource m_ViceSE;
    [SerializeField, Header("SE")]
    private AudioSource m_SE;
    #endregion
    [EndFoldout]
    private bool isHit = false;
    private bool isRide = false;
    [Tab("�h���b�v�A�C�e��")]
    [SerializeField, Header("�h���b�v�A�C�e�����X�g")]
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
        // �v���C���[�I�u�W�F�N�g��Transform���擾
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
    /// ���C�h���̏���
    /// </summary>
    private void HandleWASDMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        m_Animator.SetFloat("���E", Input.GetAxis("Horizontal"));
        m_Animator.SetFloat("�O��", Input.GetAxis("Vertical"));
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

        // ���x���ŏ��l����ő�l�͈͓̔��ɐ���
        Vector3 moveVelocity = moveDirection * Mathf.Lerp(0, 15, Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));
        m_Animator.SetBool("Ride", true);
        // �v���C���[�����[�J�����W�ňړ�
        rb.velocity = moveVelocity;
        if (isMoving)
        {
            // �J�����̕������擾���ăv���C���[�I�u�W�F�N�g����]������
            RotatePlayerWithCamera();
            if (!m_SE.isPlaying) // �Đ����łȂ��ꍇ�̂ݍĐ�
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
    /// ���C�h���̏����I��
    /// </summary>
    #region �v���C���[�ǐՏ���
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
                    // �^�[�Q�b�g�ɓ��������ꍇ
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

                    //�q�b�g���Ă���Ԃ�
                    Debug.DrawRay(raycastOrigin, directionToPlayer * hit.distance, Color.red);
                }
                else
                {
                    //�q�b�g���ĂȂ���ΐF�ɂ���
                    Debug.DrawRay(raycastOrigin, directionToPlayer * m_RaycastDistance, Color.blue);
                }
            }
        }
    }

    void PlayerTracking()
    {
        // �v���C���[�̕���������
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f; // y�������͖������Đ����Ɍ����悤�ɂ���
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSpeed);

        // �v���C���[�I�u�W�F�N�g�Ɍ������Ĉ��̑��x�ŉ���
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 desiredVelocity = direction * m_MaxSpeed;

        // ���̑��x�ňړ����邽�߂ɁA���x�𒼐ڐݒ肷��
        rb.velocity = desiredVelocity;
    }
    #endregion
    #region �v���C���[�U������
    void PlayerMagicAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // �U���������ɓ�������U������
        if (distanceToPlayer <= attackDistance)
        {
            m_MagicAttckCoolTime += Time.deltaTime;
            if (m_MagicAttckCoolTime > 5)
            {
                inAttackRange = true;
                if (!isAttacking)
                {
                    isAttacking = true;
                    // �}�W�b�N�U���̃A�j���[�V�������Đ�
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


        // �ړ������U�������O�ɂ���ꍇ�Ɉړ����s��
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
        // �v���C���[�Ƃ̋����𑪒�
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // �U���������ɓ�������U������
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


        // �ړ������U�������O�ɂ���ꍇ�Ɉړ����s��
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
        // �U���A�j���[�V�������Đ�
        m_Animator.SetBool("Attack", true);
        if (m_AttackEffect != null)
        {
            m_AttackEffect.SetActive(true);
        }
        // �U����A���̎��ԑ҂�
        yield return new WaitForSeconds(m_MeleeCoolTime);

        // �U�����I���������Ƃ������t���O�����Z�b�g
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
        // isMoving �̏�ԂɊ�Â��ăA�j���[�V������ύX
        m_Animator.SetBool("IsMoving", isMoving);
    }
    private void DropItems()
    {
        foreach (var itemInfo in dropItemsInfo)
        {
            if (Random.value <= itemInfo.dropChance)
            {
                // �A�C�e�����h���b�v����񐔂�����i�����ł͍ő�3��܂Łj
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

                // �U�����󂯂���v���C���[�̕���������
                Vector3 lookDirection = player.position - transform.position;
                lookDirection.y = 0f; // y�������͖������Đ����Ɍ����悤�ɂ���
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
        // �A�C�e�����h���b�v
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

