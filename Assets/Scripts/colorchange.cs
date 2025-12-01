using UnityEngine;
using System.Collections;

public class colorchange : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Camera.main.GetComponent<PostProcessingEnabler>().SwitchVolumes(0.5f);
        }


    }

 


// Update is called once per frame
void Update()
    {
        
    }
}
