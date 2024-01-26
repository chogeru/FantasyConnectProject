using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalRideSystem : MonoBehaviour
{
    public float m_MaxDistance = 3f; // 3mの距離
    public Transform m_PlayerObject; // プレイヤーオブジェクト
    private Transform m_RidePos; // RidePosオブジェクトのTransform
    [SerializeField, Header("ライド確認用UI")]
    private GameObject m_RideUI;
    public bool isRide = false; // ライド状態のフラグ
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
#if UNITY_EDITOR
            Debug.Log("RidePosオブジェクトが見つかってない");
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
                    Debug.LogError("EnemySystemコンポーネントが見つからない。");
                }
            }
            else
            {
                // 親オブジェクトが見つからなかった場合の処理
                Debug.LogError("RidePosの親オブジェクトが見つからない。");
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
                Debug.LogError("EnemySystemコンポーネントが見つからない");
            }
        }
        else
        {
            Debug.LogError("RidePosの親オブジェクトが見つからない");
        }
        RaycastHit hitLeft, hitRight;
        float raycastDistance = 2f; // レイキャストの距離

        bool obstacleToLeft = Physics.Raycast(m_RidePos.position, -m_RidePos.right, out hitLeft, raycastDistance);
        bool obstacleToRight = Physics.Raycast(m_RidePos.position, m_RidePos.right, out hitRight, raycastDistance);

        if (obstacleToLeft && obstacleToRight)
        {
            Debug.Log("左右に障害物があるよ");
            isRide = true;
            return; // 左右に障害物がある場合は降りられない
        }
        else if (obstacleToLeft)
        {
            MovePlayerSideways(1.5f); // 右に移動
        }
        else if (obstacleToRight)
        {
            MovePlayerSideways(-1.5f); // 左に移動
        }
        else
        {
            MovePlayerSideways(-1.5f); // 左に移動（障害物がない場合は左を優先）
        }
    }
    // 指定した方向にプレイヤーを移動する関数
    void MovePlayerSideways(float distance)
    {
        Vector3 targetPosition = m_RidePos.position + m_RidePos.right * distance;
        m_PlayerObject.position = targetPosition;
    }
}
