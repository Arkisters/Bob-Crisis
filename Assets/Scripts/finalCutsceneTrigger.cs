using UnityEngine;

public class finalCutsceneTrigger : MonoBehaviour
{



    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Cutscene2");
            Debug.Log("Cutscene triggered");
        }
    }

}
