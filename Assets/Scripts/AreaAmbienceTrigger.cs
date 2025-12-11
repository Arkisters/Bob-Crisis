using UnityEngine;

public class AreaAmbienceTrigger : MonoBehaviour
{
    [Header("Audio for area")]
    public AudioClip ambience;  // The ambience sound that should play when player enters
    public AudioClip music;     // The music that should play when player enters
    public AudioClip SFX;       // Single sfx that should play when player enters
    public AudioClip[] SFXMultiple; // Multiple sfx to play (overlapping)
    [Range(0f,1f)] public float sfxVolume = 1f;

    private AudioManager audioManager; // Reference to AudioManager

    private void Start()
    {
        // Find the AudioManager in the scene using its tag
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // Use 2D trigger callback to match the rest of the project (uses 2D physics)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger ambience/music if the thing entering is the player
        if (other.CompareTag("Player"))
        {
            // Stop any previously playing temporary SFX so long sounds don't carry across areas
            if (audioManager != null)
            {
                audioManager.StopAllSFX();
            }

            // If an ambience clip is assigned, switch to it; otherwise stop ambience
            if (audioManager != null)
            {
                var ambSource = audioManager.GetAmbienceSource();
                if (ambience != null)
                {
                    ambSource.clip = ambience;
                    ambSource.Play();
                }
                else if (ambSource != null)
                {
                    ambSource.Stop();
                    ambSource.clip = null;
                }

                // If a music clip is assigned, switch to it; otherwise stop music
                var musicSource = audioManager.GetMusicSource();
                if (music != null)
                {
                    musicSource.clip = music;
                    musicSource.Play();
                }
                else if (musicSource != null)
                {
                    musicSource.Stop();
                    musicSource.clip = null;
                }
            }

            // Play single SFX
            if (SFX != null && audioManager != null)
            {
                Debug.Log($"[AreaAmbienceTrigger] '{gameObject.name}' playing single SFX: '{SFX.name}' with volume {sfxVolume}");
                audioManager.PlaySFX(SFX, sfxVolume);
            }

            // Play multiple overlapping SFX
            if (SFXMultiple != null && SFXMultiple.Length > 0 && audioManager != null)
            {
                string names = "";
                foreach (var c in SFXMultiple)
                {
                    if (c == null) continue;
                    names += c.name + ", ";
                }
                Debug.Log($"[AreaAmbienceTrigger] '{gameObject.name}' playing multiple SFX: {names} volume {sfxVolume}");
                audioManager.PlaySFXMultiple(SFXMultiple, sfxVolume);
            }
        }
    }
}
