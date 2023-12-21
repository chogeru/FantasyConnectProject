using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Transform player; // プレイヤーのTransform
    public float sensitivity = 2.0f; // カメラの感度
    public float minDistance = 1.0f; // カメラとプレイヤーの最小距離
    public LayerMask obstacleMask; // 障害物として扱うレイヤー

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - player.position; // カメラの位置とプレイヤーの位置の差分を取得
        Cursor.visible = false;
    }

    void Update()
    {
        Cursor.visible = false;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity; // マウスのX軸の移動量
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity; // マウスのY軸の移動量

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

        transform.LookAt(player.position); // プレイヤーを見るようにカメラの向きを設定
    }
}
