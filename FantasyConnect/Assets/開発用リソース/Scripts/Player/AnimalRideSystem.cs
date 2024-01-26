using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalRideSystem : MonoBehaviour
{
    public float m_MaxDistance = 3f; // 3m�̋���
    public Transform m_PlayerObject; // �v���C���[�I�u�W�F�N�g
    private Transform m_RidePos; // RidePos�I�u�W�F�N�g��Transform
    [SerializeField, Header("���C�h�m�F�pUI")]
    private GameObject m_RideUI;
    public bool isRide = false; // ���C�h��Ԃ̃t���O
    [SerializeField]
    PlayerSystem playerSystem;
    [SerializeField]
    private GrounderFBBIK grounderFBBIK;
    private Collider myCollider;
    private EnemySystem enemySystem;
    void Start()
    {
        myCollider = GetComponent<Collider>();
        m_PlayerObject = this.gameObject.transform;
        // RidePos�I�u�W�F�N�g��T��
        GameObject ridePosObject = GameObject.FindGameObjectWithTag("RidePos");
        if (ridePosObject != null)
        {
            m_RidePos = ridePosObject.transform;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("RidePos�I�u�W�F�N�g���������ĂȂ�");
#endif
        }
    }

    void Update()
    {
        if (m_RidePos != null && Vector3.Distance(m_PlayerObject.position, m_RidePos.position) <= m_MaxDistance)
        {
            m_RideUI.SetActive(true);
        }
        else
        {
            m_RideUI.SetActive(false);
        }
        // R�L�[�������ꂽ�Ƃ�
        if (Input.GetKeyDown(KeyCode.R))
        {
            // RidePos�I�u�W�F�N�g�����݂��A�v���C���[�I�u�W�F�N�g�Ƃ̋�����maxDistance�ȓ��̏ꍇ
            if (m_RidePos != null && Vector3.Distance(m_PlayerObject.position, m_RidePos.position) <= m_MaxDistance)
            {
                // isRide�t���O���g�O��
                isRide = !isRide;

                if (isRide)
                {
                    // �v���C���[�I�u�W�F�N�g�̈ʒu�Ɖ�]��RidePos�ɍ��킹��
                    m_PlayerObject.position = m_RidePos.position;
                    m_PlayerObject.rotation = m_RidePos.rotation;
                    playerSystem.m_PlayerAnimator.SetBool("NormalAttack", false);
                    playerSystem.m_PlayerAnimator.SetBool("StrongAttack", false);

                    if (myCollider != null)
                    {
                        myCollider.isTrigger = true;
                    }
                }
                else
                {
                    EndRide();
                }
            }
        }

        if (isRide && m_RidePos != null)
        {
            grounderFBBIK.enabled = false;
            playerSystem.m_PlayerAnimator.SetBool("Ride", true);
            m_PlayerObject.position = m_RidePos.position;
            m_PlayerObject.rotation = m_RidePos.rotation;

            // RidePos�̐e�I�u�W�F�N�g���擾
            Transform parentObject = m_RidePos.parent;

            if (parentObject != null)
            {
                // EnemySystem�R���|�[�l���g���擾
                enemySystem = parentObject.GetComponent<EnemySystem>();

                if (enemySystem != null)
                {
                    // EnemySystem�R���|�[�l���g�����������ꍇ�̏���
                    enemySystem.SetIsRide(true);
                }
                else
                {
                    // EnemySystem�R���|�[�l���g��������Ȃ������ꍇ�̏���
                    Debug.LogError("EnemySystem�R���|�[�l���g��������Ȃ��B");
                }
            }
            else
            {
                // �e�I�u�W�F�N�g��������Ȃ������ꍇ�̏���
                Debug.LogError("RidePos�̐e�I�u�W�F�N�g��������Ȃ��B");
            }
        }
    }
    void EndRide()
    {
        // ���C�h�������̏������܂Ƃ߂����\�b�h

        grounderFBBIK.enabled = true; // GrounderFBBIK��L���ɂ���

        if (myCollider != null)
        {
            myCollider.isTrigger = false;
        }

        // isRide�t���O���I�t�ɂ���
        isRide = false;

        playerSystem.m_PlayerAnimator.SetBool("Ride", false); // �A�j���[�V�����̐ݒ���I�t�ɂ���

        if (m_RidePos != null && m_RidePos.parent != null)
        {
            // RidePos�̐e�I�u�W�F�N�g���擾
            Transform parentObject = m_RidePos.parent;

            // EnemySystem�R���|�[�l���g���擾����isRide��false�ɂ���
            EnemySystem enemySystem = parentObject.GetComponent<EnemySystem>();
            if (enemySystem != null)
            {
                enemySystem.SetIsRide(false);
            }
            else
            {
                Debug.LogError("EnemySystem�R���|�[�l���g��������Ȃ�");
            }
        }
        else
        {
            Debug.LogError("RidePos�̐e�I�u�W�F�N�g��������Ȃ�");
        }
        RaycastHit hitLeft, hitRight;
        float raycastDistance = 2f; // ���C�L���X�g�̋���

        bool obstacleToLeft = Physics.Raycast(m_RidePos.position, -m_RidePos.right, out hitLeft, raycastDistance);
        bool obstacleToRight = Physics.Raycast(m_RidePos.position, m_RidePos.right, out hitRight, raycastDistance);

        if (obstacleToLeft && obstacleToRight)
        {
            Debug.Log("���E�ɏ�Q���������");
            isRide = true;
            return; // ���E�ɏ�Q��������ꍇ�͍~����Ȃ�
        }
        else if (obstacleToLeft)
        {
            MovePlayerSideways(1.5f); // �E�Ɉړ�
        }
        else if (obstacleToRight)
        {
            MovePlayerSideways(-1.5f); // ���Ɉړ�
        }
        else
        {
            MovePlayerSideways(-1.5f); // ���Ɉړ��i��Q�����Ȃ��ꍇ�͍���D��j
        }
    }
    // �w�肵�������Ƀv���C���[���ړ�����֐�
    void MovePlayerSideways(float distance)
    {
        Vector3 targetPosition = m_RidePos.position + m_RidePos.right * distance;
        m_PlayerObject.position = targetPosition;
    }
}
