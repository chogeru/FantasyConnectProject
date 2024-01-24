using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField,Header("カメラ感度")]
    private float sensitivity = 2.0f; 
    [SerializeField,Header("カメラとプレイヤーの最小距離")]
    private float minDistance = 1.0f;
    [SerializeField]
    public float maxEnemyDistance = 15.0f; 
    [SerializeField,Header("障害物として扱うレイヤー")]
    public LayerMask obstacleMask;
    [SerializeField]
    private float sphereCastRadius = 0.5f;

    private Vector3 offset;
    [SerializeField]
    private Vector3 position;
    public bool isStop = false;

    void Start()
    {
        offset = transform.position - player.position; 
        Cursor.visible = false;
    }

    void Update()
    {
        // 当たり判定中でなければカメラを動かす
        if (!isStop) 
        {
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * sensitivity; // マウスのX軸の移動量
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity; // マウスのY軸の移動量
            Vector2 rightStickInput = Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero;
            mouseX += rightStickInput.x * sensitivity;
            mouseY += rightStickInput.y * sensitivity;
            // プレイヤーを中心にカメラを回転させる
            offset = Quaternion.AngleAxis(mouseX, Vector3.up) * Quaternion.AngleAxis(-mouseY, transform.right) * offset;
            transform.position = player.position + offset;

            // カメラの垂直方向の回転制限
            Vector3 cameraDirection = transform.position - player.position;
            float angleX = Vector3.Angle(cameraDirection, player.up);
            if ((angleX + mouseY) < 90f && (angleX + mouseY) > 10f)
            {
                transform.RotateAround(player.position, transform.right, -mouseY);
            }

            // カメラが壁や障害物に衝突しないようにする
            RaycastHit hit;
            if (Physics.Raycast(player.position, offset, out hit, offset.magnitude, obstacleMask))
            {
                transform.position = hit.point - offset.normalized * minDistance;
            }

            // 敵との距離が指定範囲以内の場合、カメラを移動
            if (CheckEnemyDistance())
            {
            
                RaycastHit sphereHit;
                if (Physics.SphereCast(player.position, sphereCastRadius, offset.normalized, out sphereHit, maxEnemyDistance, obstacleMask))
                {
                    transform.position = sphereHit.point - offset.normalized * minDistance;
                }
                else
                {
            
                    transform.position += offset.normalized * 1f;
                }
            }

            transform.LookAt(player.position); // プレイヤーを見るようにカメラの向きを設定
        }
    }

    bool CheckEnemyDistance()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < maxEnemyDistance)
            {
                return true;
            }
        }
        return false;
    }
}
