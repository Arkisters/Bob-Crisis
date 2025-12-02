using UnityEngine;

public class magnetToggle : MonoBehaviour
{

    public GameObject[] magnetsToToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject magnet in magnetsToToggle)
            {
                magnet.gameObject.SetActive(!magnet.activeSelf);
            }
        }
    }

}
