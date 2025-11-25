using UnityEngine;

public class TutorialSolved : MonoBehaviour
{

    public GameObject wall;
    private bool puzzleSolved = false;

    private void Start()
    {
        wall.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Props") && other.transform.position.x >= 1.42f)
        {
                puzzleSolved = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (puzzleSolved == true)
        {
            wall.SetActive(true);
        }
    }

}
