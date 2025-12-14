using UnityEngine;

public class TarSurface : MonoBehaviour
{

    public bool isMovingInTar = false;
    [SerializeField] private Rigidbody2D rb;

    public void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.CompareTag("Tar")) 
        {
            //change variable in the player controller to true
            isMovingInTar = true;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Tar"))
        {
            //change variable in the player controller to false
            isMovingInTar = false;
        }
    }


    private void FixedUpdate()
    {
        if (isMovingInTar)
        {
            //Debug.Log("Player is moving in tar");
            rb.linearVelocity = rb.linearVelocity * 0.5f;
        }
    }
}
