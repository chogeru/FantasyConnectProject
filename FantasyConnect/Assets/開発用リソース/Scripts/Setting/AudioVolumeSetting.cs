using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Audio;

public class AudioVolumeSetting : MonoBehaviour
{
    [SerializeField] 
    AudioMixer m_AudioMixer;
    [SerializeField]
    Slider m_BGMSlider;
    [SerializeField] 
    Slider m_SESlider;

    private void Start()
    {

        m_AudioMixer.GetFloat("BGM", out float bgmVolume);
        m_BGMSlider.value = bgmVolume;

        m_AudioMixer.GetFloat("SE", out float seVolume);
        m_SESlider.value = seVolume;
    }

    public void SetBGM(float volume)
    {
        m_AudioMixer.SetFloat("BGM", volume);
    }

    public void SetSE(float volume)
    {
        m_AudioMixer.SetFloat("SE", volume);
    }
}
