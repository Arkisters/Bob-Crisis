using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class healthVignette : MonoBehaviour
{
    [SerializeField] private Volume vignetteVolume;
    [SerializeField] private Health playerHealth;
    [SerializeField] private float minVignetteIntensity = 0f;
    [SerializeField] private float maxVignetteIntensity = 0.8f;
    
    private Vignette vignette;

    private void Start()
    {
        // Get the Vignette component from the volume
        if (vignetteVolume != null && vignetteVolume.profile.TryGet(out vignette))
        {
            // Ensure the intensity override is enabled so the volume system applies it
            vignette.intensity.overrideState = true;
            // Optionally ensure color override is enabled (keeps vignette dark)
            vignette.color.overrideState = true;
        }

        // If playerHealth is not assigned, try to find it
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<Health>();
        }
    }

    private void Update()
    {
        if (playerHealth == null || vignette == null)
        {
            return;
        }

        // Ensure the volume is fully weighted so the profile is applied
        if (vignetteVolume != null)
        {
            vignetteVolume.weight = 1f;
        }

        // Calculate health ratio (0 = dead, 1 = full health)
        float healthRatio = playerHealth.health / playerHealth.maxHealth;

        // Invert so vignette is stronger when health is lower
        // 0 health = max vignette, full health = min vignette
        float vignetteIntensity = Mathf.Lerp(maxVignetteIntensity, minVignetteIntensity, healthRatio);
        // Apply the intensity to the vignette and ensure override is enabled
        vignette.intensity.overrideState = true;
        vignette.intensity.value = Mathf.Clamp01(vignetteIntensity);
    }
}
