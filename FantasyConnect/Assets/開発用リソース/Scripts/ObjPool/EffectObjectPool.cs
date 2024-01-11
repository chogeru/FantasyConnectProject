using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObjectPool : MonoBehaviour
{
    public static EffectObjectPool Instance; // シングルトンインスタンス

    public GameObject effectPrefab; // エフェクトのPrefab

    public int poolSize = 10; // オブジェクトプールのサイズ

    private List<GameObject> pooledObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializePool();
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject effect = Instantiate(effectPrefab);
            effect.SetActive(false);
            effect.transform.parent = transform;
            pooledObjects.Add(effect);
        }
    }

    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // プール内でアクティブなオブジェクトがない場合、新たに生成して返す
        GameObject newObj = Instantiate(effectPrefab);
        newObj.SetActive(true);
        pooledObjects.Add(newObj);
        return newObj;
    }

    public void ReturnPooledObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}
