using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-----Audio Sources-----")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource ambienceSource;

    [Header("-----Audio Clips-----")]
    public AudioClip GlorpMain;
    public AudioClip Glorp2;
    public AudioClip GlorpFactAmb;
    public AudioClip GlorpOutAmb;
    public AudioClip BlockGrabFromActiveMagnet;
    public AudioClip GlorpReggaeton;

    private void Start()
    {
        ambienceSource.clip = GlorpOutAmb;
        ambienceSource.Play();
        musicSource.clip = GlorpReggaeton;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

}
