using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBossLook : MonoBehaviour
{
    [SerializeField]
    private Transform m_BossTF;
 

    // Update is called once per frame
    void Update()
    {
     transform.LookAt(m_BossTF.position);   
    }
}
