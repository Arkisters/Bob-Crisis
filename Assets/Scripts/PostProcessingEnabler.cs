using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public enum VolumeMode
{
    Both,           // Can switch to either green or normal
    GreenOnly,      // Only switch to green volume
    NormalOnly      // Only switch to normal volume
}

public class PostProcessingEnabler : MonoBehaviour
{
    [SerializeField] private Volume baseVolume, greenVolume;
    [SerializeField] private VolumeMode volumeMode = VolumeMode.Both;
    private Coroutine switchCoroutine;

    public void SetVolumeMode(VolumeMode mode)
    {
        volumeMode = mode;
    }

    public void SwitchVolumes(float speed)
    {
        bool shouldSwitchToGreen = baseVolume.weight <= 0;

        // Check if this switch is allowed based on volumeMode
        if (volumeMode == VolumeMode.GreenOnly && shouldSwitchToGreen)
        {
            return;
        }
        if (volumeMode == VolumeMode.NormalOnly && !shouldSwitchToGreen)
        {
            return;
        }

        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
        }

        switchCoroutine = StartCoroutine(SwitchVolumes_(speed, shouldSwitchToGreen));
    }

    private IEnumerator SwitchVolumes_(float speed, bool switchFromGreenVolume)
    {
        Volume selectedVol = switchFromGreenVolume ? baseVolume : greenVolume;
        Volume notSelectedVol = !switchFromGreenVolume ? baseVolume : greenVolume;

        while(selectedVol.weight < 1)
        {
            selectedVol.weight += Time.deltaTime * speed;
            notSelectedVol.weight -= Time.deltaTime * speed;
            yield return null;
        }

        selectedVol.weight = 1;
        notSelectedVol.weight = 0;

        switchCoroutine = null;
    }
}
