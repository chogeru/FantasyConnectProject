using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cat 
{
    [RequireComponent(typeof(AudioSource))]
    public class MonsterSE : MonoBehaviour
    { 
        [SerializeField,Header("モンスターの鳴き声")]
        private AudioSource m_AudioSouce;
        [SerializeField,Header("鳴き声の音声クリップ")]
        private AudioClip[] MonsterSounds;
        private float m_SECoolTime;
        [SerializeField]
        private float m_Time;
        private void Update()
        {
            m_SECoolTime += Time.deltaTime;
            m_Time=Random.Range(0, 20);
            if(m_SECoolTime > m_Time)
            {
                int randomSoundIndex = Random.Range(0, MonsterSounds.Length);
                m_AudioSouce.clip = MonsterSounds[randomSoundIndex];
                m_AudioSouce.Play();
                m_SECoolTime = 0;
            }    
        }
        
     
        
    }
}
