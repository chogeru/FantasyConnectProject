using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveWall : MonoBehaviour
{
    [SerializeField, Header("�I�u�W�F�N�g�̃��X�g")]
    [Tooltip("���g�̃I�u�W�F�N�g���폜��������ɂȂ�I�u�W�F�N�g��ǉ�")]
    private List<GameObject> m_Obj = new List<GameObject>();

    [SerializeField, Header("Dissolve Shader Material")]
    [Tooltip("Dissolve Shader��K�p����Material")]
    private Material m_OriginalDissolveMaterial;
    [SerializeField,Header("���������}�e���A��(�����Ƀ}�e���A���͐ݒ肵�Ȃ�)")]
    private Material m_CurrentDissolveMaterial;
    private bool isDissolving = false;

    private void Start()
    {
        SetMaterial();
    }

    void Update()
    {
        // �I�u�W�F�N�g���Ȃ����m�F
        CheckEnemies();
    }
    private void SetMaterial()
    {
        m_CurrentDissolveMaterial = new Material(m_OriginalDissolveMaterial);
        m_CurrentDissolveMaterial.SetFloat("_DissolveAmount", 0.0f);
        GetComponent<Renderer>().material = m_CurrentDissolveMaterial;
    }

    void CheckEnemies()
    {
        // ���X�g���̃I�u�W�F�N�g�����ׂĂȂ��Ȃ����玩�g�̃I�u�W�F�N�g��j��
        if (m_Obj.Count == 0 && !isDissolving)
        {
            StartCoroutine(DissolveAndDestroy());
        }
        else
        {
            // ���X�g���̃I�u�W�F�N�g�����݂��邩�m�F���A�Ȃ����̂̓��X�g����폜
            m_Obj.RemoveAll(obj => obj == null);
        }
    }

    IEnumerator DissolveAndDestroy()
    {
        isDissolving = true;

        float startTime = Time.time;
        float duration = 5.0f;

        while (Time.time - startTime < duration)
        {
            float progress = (Time.time - startTime) / duration;
            m_CurrentDissolveMaterial.SetFloat("_DissolveAmount", progress);

            yield return null;
        }

        m_CurrentDissolveMaterial.SetFloat("_DissolveAmount", 1.0f);

        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }
}
