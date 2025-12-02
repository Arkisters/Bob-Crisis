using UnityEngine;

public class TarSurface : MonoBehaviour
{

    public bool isMovingInTar = false;

    public void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            //change variable in the player controller to true
            //collision.GetComponent<PlayerController>().isMovingInTar = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            //change variable in the player controller to false
            //collision.GetComponent<PlayerController>().isMovingInTar = true;
        }
    }
}
