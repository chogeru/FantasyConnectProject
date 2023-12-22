using UnityEngine;

public class PlayerIconAnime : MonoBehaviour
{
    private Animator m_IConAnimator;
    public bool isHit=false;
    void Start()
    {
        m_IConAnimator= GetComponent<Animator>();
    }

    void Update()
    {
     m_IConAnimator.SetBool("isHit", isHit);
    }
    private void EndAnime()
    {
        isHit=false;
        m_IConAnimator.SetBool("isHit", false);
    }
}
