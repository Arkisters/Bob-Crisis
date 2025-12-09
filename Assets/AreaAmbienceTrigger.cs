using UnityEngine;

public class AreaAmbienceTrigger : MonoBehaviour
{
    [Header("Audio for area")]
    public AudioClip ambience;  // The ambience sound that should play when player enters
    public AudioClip music;     // The music that should play when player enters

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
            // If an ambience clip is assigned, switch to it
            if (ambience != null)
            {
                audioManager.GetAmbienceSource().clip = ambience;
                audioManager.GetAmbienceSource().Play();
            }

            // If a music clip is assigned, switch to it
            if (music != null)
            {
                audioManager.GetMusicSource().clip = music;
                audioManager.GetMusicSource().Play();
            }
        }
    }
}
