using UnityEngine;
using System.Collections;

public class colorchange : MonoBehaviour
{
    [SerializeField] private VolumeMode volumeMode = VolumeMode.Both;
    [SerializeField] private TilemapSwapper tilemapSwapper;
    
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            // Trigger post processing volume change
            PostProcessingEnabler enabler = Camera.main.GetComponent<PostProcessingEnabler>();
            if (enabler != null)
            {
                enabler.SetVolumeMode(volumeMode);
                enabler.SwitchVolumes(0.5f);
            }
            
            // Trigger tilemap freshness increase
            if (tilemapSwapper != null)
            {
                tilemapSwapper.IncreaseFreshness();
            }
            else
            {
                Debug.LogWarning("colorchange: TilemapSwapper reference not assigned!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
