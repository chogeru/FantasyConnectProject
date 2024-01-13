using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    public float moveSpeed = 5f;  // 移動速度

    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 入力を取得
        float moveInput = Input.GetAxis("Horizontal");

        // 移動方向に速度を設定
        Vector2 moveVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = moveVelocity;

        // アニメーションの制御
        //移動時はmoveInputが0以上になる
        //左移動時は0以下に
        if (moveInput > 0)
        {
            // 右に移動
            animator.SetBool("前移動", true);
            animator.SetBool("後ろ移動", false);
        }
        else if (moveInput < 0)
        {
            // 左に移動
            animator.SetBool("前移動", false);
            animator.SetBool("後ろ移動", true);
        }
        else
        {
            // 移動していない
            animator.SetBool("前移動", false);
            animator.SetBool("後ろ移動", false);
        }
        

    }
}