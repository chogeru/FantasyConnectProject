using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyDestroySignal : MonoBehaviour
{
    [SerializeField]
    private BossEnemyManager bossEnemyManager;
    private void OnDestroy()
    {
        if(bossEnemyManager!= null)
        {
            bossEnemyManager.DestroyBossEnemy(gameObject);
        }
    }
}
