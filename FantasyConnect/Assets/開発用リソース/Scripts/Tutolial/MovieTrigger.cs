using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovieTrigger : MonoBehaviour
{
   public List<GameObject> m_DeActiveObj;
    public GameObject m_TimelineObj;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            foreach (GameObject obj in m_DeActiveObj)
            {
                obj.SetActive(false);
            }

            m_TimelineObj.SetActive(true); 
        }
    }
}
