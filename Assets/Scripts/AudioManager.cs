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
        if (ambienceSource != null)
        {
            ambienceSource.clip = GlorpOutAmb;
            ambienceSource.Play();
        }
        if (musicSource != null)
        {
            musicSource.clip = GlorpReggaeton;
            musicSource.Play();
        }
    }

    // Play a single SFX using PlayOneShot (allows overlapping)
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || SFXSource == null) return;
        SFXSource.PlayOneShot(clip);
    }

    // Play SFX with explicit volume (0..1)
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || SFXSource == null) return;
        SFXSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    // Play multiple SFX at once
    public void PlaySFXMultiple(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || SFXSource == null) return;
        float v = Mathf.Clamp01(volume);
        foreach (var c in clips)
        {
            if (c == null) continue;
            SFXSource.PlayOneShot(c, v);
        }
    }

    // Stop all SFX currently playing on the SFX source
    public void StopAllSFX()
    {
        if (SFXSource == null) return;
        SFXSource.Stop();
    }

    // Accessors
    public AudioSource GetAmbienceSource() => ambienceSource;
    public AudioSource GetMusicSource() => musicSource;
    public AudioSource GetSFXSource() => SFXSource;
}
