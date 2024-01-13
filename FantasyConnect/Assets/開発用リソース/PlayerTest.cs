using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    public float moveSpeed = 5f;  // �ړ����x

    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ���͂��擾
        float moveInput = Input.GetAxis("Horizontal");

        // �ړ������ɑ��x��ݒ�
        Vector2 moveVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = moveVelocity;

        // �A�j���[�V�����̐���
        //�ړ�����moveInput��0�ȏ�ɂȂ�
        //���ړ�����0�ȉ���
        if (moveInput > 0)
        {
            // �E�Ɉړ�
            animator.SetBool("�O�ړ�", true);
            animator.SetBool("���ړ�", false);
        }
        else if (moveInput < 0)
        {
            // ���Ɉړ�
            animator.SetBool("�O�ړ�", false);
            animator.SetBool("���ړ�", true);
        }
        else
        {
            // �ړ����Ă��Ȃ�
            animator.SetBool("�O�ړ�", false);
            animator.SetBool("���ړ�", false);
        }
        

    }
}