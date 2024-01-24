using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField,Header("�J�������x")]
    private float sensitivity = 2.0f; 
    [SerializeField,Header("�J�����ƃv���C���[�̍ŏ�����")]
    private float minDistance = 1.0f;
    [SerializeField]
    public float maxEnemyDistance = 15.0f; 
    [SerializeField,Header("��Q���Ƃ��Ĉ������C���[")]
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
        // �����蔻�蒆�łȂ���΃J�����𓮂���
        if (!isStop) 
        {
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * sensitivity; // �}�E�X��X���̈ړ���
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity; // �}�E�X��Y���̈ړ���
            Vector2 rightStickInput = Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero;
            mouseX += rightStickInput.x * sensitivity;
            mouseY += rightStickInput.y * sensitivity;
            // �v���C���[�𒆐S�ɃJ��������]������
            offset = Quaternion.AngleAxis(mouseX, Vector3.up) * Quaternion.AngleAxis(-mouseY, transform.right) * offset;
            transform.position = player.position + offset;

            // �J�����̐��������̉�]����
            Vector3 cameraDirection = transform.position - player.position;
            float angleX = Vector3.Angle(cameraDirection, player.up);
            if ((angleX + mouseY) < 90f && (angleX + mouseY) > 10f)
            {
                transform.RotateAround(player.position, transform.right, -mouseY);
            }

            // �J�������ǂ��Q���ɏՓ˂��Ȃ��悤�ɂ���
            RaycastHit hit;
            if (Physics.Raycast(player.position, offset, out hit, offset.magnitude, obstacleMask))
            {
                transform.position = hit.point - offset.normalized * minDistance;
            }

            // �G�Ƃ̋������w��͈͈ȓ��̏ꍇ�A�J�������ړ�
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

            transform.LookAt(player.position); // �v���C���[������悤�ɃJ�����̌�����ݒ�
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
