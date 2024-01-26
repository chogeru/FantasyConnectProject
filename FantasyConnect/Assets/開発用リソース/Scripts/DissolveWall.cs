using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveWall : MonoBehaviour
{
    [SerializeField, Header("オブジェクトのリスト")]
    [Tooltip("自身のオブジェクトを削除する条件になるオブジェクトを追加")]
    private List<GameObject> m_Obj = new List<GameObject>();

    [SerializeField, Header("Dissolve Shader Material")]
    [Tooltip("Dissolve Shaderを適用したMaterial")]
    private Material m_OriginalDissolveMaterial;
    [SerializeField,Header("複製したマテリアル(ここにマテリアルは設定しない)")]
    private Material m_CurrentDissolveMaterial;
    private bool isDissolving = false;

    private void Start()
    {
        SetMaterial();
    }

    void Update()
    {
        // オブジェクトがないか確認
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
        // リスト内のオブジェクトがすべてなくなったら自身のオブジェクトを破棄
        if (m_Obj.Count == 0 && !isDissolving)
        {
            StartCoroutine(DissolveAndDestroy());
        }
        else
        {
            // リスト内のオブジェクトが存在するか確認し、ないものはリストから削除
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
