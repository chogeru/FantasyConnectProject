using MagicaCloth2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMagicWeapon : MonoBehaviour
{
    [SerializeField]
    EnemySystem enemySystem;
    public GameObject bulletPrefab; // プレハブとして使用する魔法弾のオブジェクト
    public Transform firePoint; // 銃口として使用するオブジェクト
    public bool isMagicAttck = false;
    public float bulletSpeed = 100f; // 魔法弾の速度
    [SerializeField]
    private Animator animator;
    private Transform player; // プレイヤーのTransform

    void Start()
    {
        // プレイヤーのTransformを取得
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        ActivateMagicAttack();
    }
    public void ActivateMagicAttack()
    {
        if (isMagicAttck && player != null)
        {
            // プレイヤーの方向を計算
            Vector3 direction = (player.position+Vector3.up*0.5f - firePoint.position).normalized;

            // 魔法弾を生成してプレイヤーの方向に発射
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * bulletSpeed;
            }
            animator.SetBool("MagicAttack", false);
            enemySystem.m_MagicAttckCoolTime = 0;
            enemySystem.isAttacking = false;
            isMagicAttck = false;
        }
    }
}
