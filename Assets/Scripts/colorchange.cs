using UnityEngine;
using System.Collections;

public class colorchange : MonoBehaviour
{
    [SerializeField] private VolumeMode volumeMode = VolumeMode.Both;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PostProcessingEnabler enabler = Camera.main.GetComponent<PostProcessingEnabler>();
            if (enabler == null)
            {
                return;
            }
            enabler.SetVolumeMode(volumeMode);
            enabler.SwitchVolumes(0.5f);
        }


    }

 


// Update is called once per frame
void Update()
    {
        
    }
}
