using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalRideSystem : MonoBehaviour
{
    public float m_MaxDistance = 3f; // 3mの距離
    public Transform m_PlayerObject; // プレイヤーオブジェクト
    private Transform m_RidePos; // RidePosオブジェクトのTransform
    private bool isRide = false; // ライド状態のフラグ
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
        // RidePosオブジェクトを探す
        GameObject ridePosObject = GameObject.FindGameObjectWithTag("RidePos");
        if (ridePosObject != null)
        {
            m_RidePos = ridePosObject.transform;
        }
        else
        {
            Debug.LogError("RidePosオブジェクトが見つかりませんでした。");
        }
    }

    void Update()
    {
        // Rキーが押されたとき
        if (Input.GetKeyDown(KeyCode.R))
        {
            // RidePosオブジェクトが存在し、プレイヤーオブジェクトとの距離がmaxDistance以内の場合
            if (m_RidePos != null && Vector3.Distance(m_PlayerObject.position, m_RidePos.position) <= m_MaxDistance)
            {
                // isRideフラグをトグル
                isRide = !isRide;

                if (isRide)
                {
                    // プレイヤーオブジェクトの位置と回転をRidePosに合わせる
                    m_PlayerObject.position = m_RidePos.position;
                    m_PlayerObject.rotation = m_RidePos.rotation;
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

            // RidePosの親オブジェクトを取得
            Transform parentObject = m_RidePos.parent;

            if (parentObject != null)
            {
                // EnemySystemコンポーネントを取得
                 enemySystem = parentObject.GetComponent<EnemySystem>();

                if (enemySystem != null)
                {
                    // EnemySystemコンポーネントが見つかった場合の処理
                    enemySystem.SetIsRide(true);
                }
                else
                {
                    // EnemySystemコンポーネントが見つからなかった場合の処理
                    Debug.LogError("EnemySystemコンポーネントが見つかりませんでした。");
                }
            }
            else
            {
                // 親オブジェクトが見つからなかった場合の処理
                Debug.LogError("RidePosの親オブジェクトが見つかりませんでした。");
            }
        }
    }
    void EndRide()
    {
        // ライド解除時の処理をまとめたメソッド

        grounderFBBIK.enabled = true; // GrounderFBBIKを有効にする

        if (myCollider != null)
        {
            myCollider.isTrigger = false;
        }

        // isRideフラグをオフにする
        isRide = false;

        playerSystem.m_PlayerAnimator.SetBool("Ride", false); // アニメーションの設定をオフにする

        if (m_RidePos != null && m_RidePos.parent != null)
        {
            // RidePosの親オブジェクトを取得
            Transform parentObject = m_RidePos.parent;

            // EnemySystemコンポーネントを取得してisRideをfalseにする
            EnemySystem enemySystem = parentObject.GetComponent<EnemySystem>();
            if (enemySystem != null)
            {
                enemySystem.SetIsRide(false);
            }
            else
            {
                Debug.LogError("EnemySystemコンポーネントが見つかりませんでした。");
            }
        }
        else
        {
            Debug.LogError("RidePosの親オブジェクトが見つかりませんでした。");
        }
    }
}
