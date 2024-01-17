using MagicaCloth2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMagicWeapon : MonoBehaviour
{
    [SerializeField]
    EnemySystem enemySystem;
    public GameObject bulletPrefab; // �v���n�u�Ƃ��Ďg�p���閂�@�e�̃I�u�W�F�N�g
    public Transform firePoint; // �e���Ƃ��Ďg�p����I�u�W�F�N�g
    public bool isMagicAttck = false;
    public float bulletSpeed = 100f; // ���@�e�̑��x
    [SerializeField]
    private Animator animator;
    private Transform player; // �v���C���[��Transform

    void Start()
    {
        // �v���C���[��Transform���擾
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
            // �v���C���[�̕������v�Z
            Vector3 direction = (player.position+Vector3.up*0.5f - firePoint.position).normalized;

            // ���@�e�𐶐����ăv���C���[�̕����ɔ���
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
