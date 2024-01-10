using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoveryItem : MonoBehaviour
{
    enum ItemType
    {
        HP,
        MP,
    }
    [SerializeField,Header("�A�C�e���̎��")]
    private ItemType m_itemType;
    [SerializeField, Header("HP�񕜗�")]
    private int m_HPRecover;
    [SerializeField,Header("MP�񕜗�")]
    private int m_MPRecover;
    [SerializeField, Header("�ڐG���̃G�t�F�N�g")]
    private GameObject m_HitEffect;

    // �I�u�W�F�N�g�v�[���}�l�[�W���ւ̎Q��
    private ItemEffectRecoveryObjctPool effectObjectPool;
    private void Start()
    {
        // ItemEffectObjctPool �̃V���O���g���C���X�^���X���擾
        effectObjectPool = ItemEffectRecoveryObjctPool.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (m_itemType == ItemType.HP)
            {
                PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();

                // �I�u�W�F�N�g�v�[������G�t�F�N�g���擾
                GameObject hitEffect = effectObjectPool.GetPooledObject();
                hitEffect.transform.position = transform.position;
                hitEffect.SetActive(true);

                playerSystem.HpRecovery(m_HPRecover);
                Destroy(gameObject);
            }
            else if (m_itemType == ItemType.MP)
            {
                PlayerSystem playerSystem = other.GetComponent<PlayerSystem>();

                // �I�u�W�F�N�g�v�[������G�t�F�N�g���擾
                GameObject hitEffect = effectObjectPool.GetPooledObject();
                hitEffect.transform.position = transform.position;
                hitEffect.SetActive(true);

                playerSystem.MPRecovery(m_MPRecover);
                Destroy(gameObject);
            }
        }
    }
}

