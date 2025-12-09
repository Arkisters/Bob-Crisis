using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-----Audio Sources-----")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource ambienceSource;

    [Header("-----Audio Clips-----")]
    public AudioClip GlorpMain;
    public AudioClip Glorp6;
    public AudioClip GlorpFactAmb;
    public AudioClip GlorpOutAmb;
    public AudioClip BlokPickup;
    public AudioClip GlorpReggaeton;
    public AudioClip BlokPutDown;
    public AudioClip ButtonPress;
    public AudioClip Footstep1;
    public AudioClip Footstep2;
    public AudioClip Footstep3;
    public AudioClip Footstep4;
    public AudioClip FactoryExplosion;
    public AudioClip PistonActivate;
    public AudioClip SawBlade;
    public AudioClip TarSludge;
    public AudioClip FireBlazing;

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

    // So other scripts can access the audio sources if needed
    public AudioSource GetAmbienceSource() => ambienceSource;
    public AudioSource GetMusicSource() => musicSource;

}
