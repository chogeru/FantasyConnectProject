using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTimelineSignal : MonoBehaviour
{
    [SerializeField, Header("削除するimageオブジェ")]
    private List<GameObject> m_ImageObjs;
    
    public void DestroyImage()
    {
      foreach(GameObject imageobj in m_ImageObjs)
        {
            imageobj.SetActive(false);
        }
    }
}
