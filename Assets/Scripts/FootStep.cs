using UnityEngine;

public class FootSteep : MonoBehaviour
{
    private AudioManager audioManager;
    private AudioClip[] footstepClips;

    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        footstepClips = new AudioClip[]
        {
        audioManager.Footstep1,
        audioManager.Footstep2,
        audioManager.Footstep3,
        audioManager.Footstep4
        };
    }
    public void PlayFootstep()
    {
        int randomIndex = Random.Range(0, footstepClips.Length);
        audioManager.PlaySFX(footstepClips[randomIndex]);
    }
}
