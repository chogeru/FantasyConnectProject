using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeponAttckCol : MonoBehaviour
{
    [SerializeField]
    private int Damage = 80;
    [SerializeField, Header("çUåÇSE")]
    private List<AudioClip> m_AttackSEClip;
    [SerializeField]
    private AudioSource m_AttckSE;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemySystem enemySystem = other.GetComponent<EnemySystem>();
            AttckSE();
            enemySystem.TakeDamage(Damage);

        }
    }
    private void AttckSE()
    {
        if (m_AttckSE != null && m_AttackSEClip.Count > 0)
        {
            int seRandamIndex = Random.Range(0, m_AttackSEClip.Count);
            AudioClip selectedSE = m_AttackSEClip[seRandamIndex];

            m_AttckSE.clip = selectedSE;
            m_AttckSE.Play();
        }
    }
}
