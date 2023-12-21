using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Transform player; // �v���C���[��Transform
    public float sensitivity = 2.0f; // �J�����̊��x
    public float minDistance = 1.0f; // �J�����ƃv���C���[�̍ŏ�����
    public LayerMask obstacleMask; // ��Q���Ƃ��Ĉ������C���[

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - player.position; // �J�����̈ʒu�ƃv���C���[�̈ʒu�̍������擾
        Cursor.visible = false;
    }

    void Update()
    {
        Cursor.visible = false;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity; // �}�E�X��X���̈ړ���
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity; // �}�E�X��Y���̈ړ���

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

        transform.LookAt(player.position); // �v���C���[������悤�ɃJ�����̌�����ݒ�
    }
}
