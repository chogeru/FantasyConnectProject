using System.Collections;
using UnityEngine;
using VInspector;
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
    [Foldout("���C�L���X�g"),Tab("���C�L���X�g")]
    #region ���C�L���X�g
    [SerializeField, Header("���C�L���X�g����")]
    private float m_RaycastDistance = 10f;
    [SerializeField, Header("���C�L���X�g�p�x")]
    private float m_Angle = 90f;
    [SerializeField, Header("���C�̐�")]
    private int rayCount = 180;
    #endregion
    [Foldout("�p�����[�^"),Tab("�p�����[�^")]
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

    [SerializeField]
    private GameObject m_HpBer;
    [SerializeField, Header("Hp�o�[�̈ʒu")]
    private Vector3 m_HpBerPos;
    [SerializeField, Header("Hp�o�[�̃T�C�Y")]
    private Vector3 m_HpBerscale;
    private Vector3 currentVelocity;
    public LayerMask obstacleMask;
    EnemyStatus enemyStatus;
    #region �T�E���h
    [Foldout("�����N���b�v"), Tab("�����N���b�v")]
    [SerializeField, Header("�q�b�g�{�C�X�N���b�v")]
    private AudioClip m_HitVoiceClip;
    [SerializeField, Header("���S���{�C�X�N���b�v")]
    private AudioClip m_DieVoiceClip;
    [SerializeField,Header("��eSE")]
    private AudioClip m_HitSEClip;
    [SerializeField, Header("���SSE")]
    private AudioClip m_DieSEClip;
    [Foldout("�I�[�f�B�I�\�[�X")]
    [SerializeField,Header("�{�C�X")]
    private AudioSource m_ViceSE;
    [SerializeField, Header("SE")]
    private AudioSource m_SE;
#endregion
    [EndFoldout]
    private bool isHit = false;

    void Start()
    {
        m_CurrentSpeed = m_MaxSpeed;
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
    #region �v���C���[�ǐՏ���
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

        // �U����A���̎��ԑ҂�
        yield return new WaitForSeconds(m_MeleeCoolTime);

        // �U�����I���������Ƃ������t���O�����Z�b�g
        isAttacking = false;
        m_Animator.SetBool("Attack", false);
        enemyAttckCol.isAttack = false;

    }
    #endregion
    void UpdateAnimation()
    {
        // isMoving �̏�ԂɊ�Â��ăA�j���[�V������ύX
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

