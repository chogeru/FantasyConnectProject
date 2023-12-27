using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowWepon : MonoBehaviour
{
    [SerializeField]
    private Animator m_PlayerAnimator;
    [SerializeField, Header("攻撃クールタイム")]
    private float m_AttckCoolTime;
    [SerializeField, Header("アニメーションから矢を発射するまでの時間")]
    private float m_ArrowShotTime;
    [SerializeField, Header("通常攻撃用矢")]
    private GameObject m_NomalArrow;
    [SerializeField, Header("強攻撃矢")]
    private GameObject m_StrongArrow;
    [SerializeField, Header("攻撃範囲")]
    private float m_AttckRange = 10;
    [SerializeField]
    private Transform m_NomalBulletSpawnPoint;
    [SerializeField]
    private Transform m_StrongBulletSpawnPoint;
    [SerializeField]
    private float bulletForce;
    private bool isAttck = false;


    [SerializeField, Header("攻撃ボイス")]
    private List<AudioClip> m_ATVoices; [SerializeField, Header("ボイス")]
    private AudioSource m_Voice;
    [SerializeField]
    private AudioSource m_ShotSE;
    void Update()
    {
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        if (playerSystem.isAttck)
        {
            ArrowAttack();
        }
        if (playerSystem.isStrongAttck)
        {
            StrongAttack();
        }
        if (Input.GetMouseButtonUp(0))
        {
            playerSystem.isWeponChange = true;
            m_PlayerAnimator.SetBool("StrongAttack", false);
            m_PlayerAnimator.SetBool("NormalAttack", false);
        }
    }
    void ArrowAttack()
    {
        m_ShotSE.Play();
        AttckSound();
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        playerSystem.isAttck = false;
        playerSystem.isStrongAttck = false;
        StopMoveAnime();
        Vector3 direction = m_NomalBulletSpawnPoint.forward; // 銃口の向きを取得する

        // 矢の向きを銃口の向きに合わせる
        Quaternion rotation = Quaternion.LookRotation(direction);

        // 一旦、上方向を銃口の上方向に設定
        Vector3 correctedUpDirection = m_NomalBulletSpawnPoint.up;

        // 矢の回転を調整して横向きにする
        rotation = Quaternion.LookRotation(direction, correctedUpDirection);

        GameObject bullet = Instantiate(m_NomalArrow, m_NomalBulletSpawnPoint.position, rotation);
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

        if (bulletRigidbody != null)
        {
            bulletRigidbody.AddForce(direction * bulletForce, ForceMode.Impulse);
        }

    }

    void StrongAttack()
    {
        m_ShotSE.Play();
        AttckSound();
        PlayerSystem playerSystem = GetComponentInParent<PlayerSystem>();
        playerSystem.isAttck = false;
        playerSystem.isStrongAttck = false;
        StopMoveAnime();

        Vector3 direction = m_StrongBulletSpawnPoint.forward; // 前方向を取得

        for (int i = 0; i < 3; i++)
        {
            Quaternion rotation = Quaternion.LookRotation(direction); // 前方向への回転を取得

            GameObject bullet = Instantiate(m_StrongArrow, m_StrongBulletSpawnPoint.position, rotation);
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

            if (bulletRigidbody != null)
            {
                // 水平に発射するための方向修正（矢を水平に広げる）
                Vector3 horizontalDirection = Quaternion.AngleAxis(-15f + (i * 15f), Vector3.up) * direction;
                bulletRigidbody.AddForce(horizontalDirection * bulletForce, ForceMode.Impulse);
            }
        }
    }

        private void StopMoveAnime()
    {
        m_PlayerAnimator.SetBool("Idle", false);
        m_PlayerAnimator.SetBool("Walk", false);
        m_PlayerAnimator.SetBool("Run", false);
    }
    private void AttckSound()
    {
        if (m_ATVoices != null && m_ATVoices.Count > 0)
        {
            int randomIndex = Random.Range(0, m_ATVoices.Count); // ランダムなインデックスを取得
            AudioClip selectedVoice = m_ATVoices[randomIndex]; // ランダムに選択された攻撃ボイスを取得

            m_Voice.clip = selectedVoice;
            m_Voice.Play();
        }
    }
}
