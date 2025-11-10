using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float walkSpeed = 5f;
    public float crouchSpeed = 2f;
    public bool isCrouching = false;
    public bool isGrounded = true;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // check wasd input, and move character accordingly
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        if (isCrouching)
        {
            transform.Translate(movement * crouchSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(movement * walkSpeed * Time.deltaTime, Space.World);
        }
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
    }
}
