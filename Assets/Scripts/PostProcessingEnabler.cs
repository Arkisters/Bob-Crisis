using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingEnabler : MonoBehaviour
{
    [SerializeField] private Volume baseVolume, greenVolume;
    private bool hasSwitched = true;

    public void SwitchVolumes(float speed)
    {
        if (!hasSwitched) return;

        StartCoroutine(SwitchVolumes_(speed, baseVolume.weight <= 0));
        hasSwitched = false;
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

        hasSwitched = true;
    }
}
