using UnityEngine;

public class ChainBehaviour : MonoBehaviour, IInteractable
{
    [Header("Chain Settings")]
    public float swingForce = 50f;
    
    private Rigidbody2D chainRigidbody;
    private PlayerController grabbingPlayer;
    private bool isBeingGrabbed = false;
    
    void Start()
    {
        chainRigidbody = GetComponent<Rigidbody2D>();
        
        if (chainRigidbody == null)
        {
            Debug.LogWarning($"ChainBehaviour on {gameObject.name} requires a Rigidbody2D component!");
        }
    }
    
    void FixedUpdate()
    {
        // Apply swinging forces when player is grabbing
        if (isBeingGrabbed && grabbingPlayer != null && chainRigidbody != null)
        {
            float horizontalInput = grabbingPlayer.GetHorizontalInput();
            
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                Vector2 swingDirection = Vector2.right * horizontalInput * swingForce;
                chainRigidbody.AddForce(swingDirection, ForceMode2D.Force);
            }
        }
    }
    
    public void Interact(PlayerController player)
    {
        if (isBeingGrabbed)
        {
            // Already grabbed, do nothing
            return;
        }
        
        // Start grab
        isBeingGrabbed = true;
        grabbingPlayer = player;
        player.SetGrabbedChain(this);
    }
    
    public void StopGrab()
    {
        if (!isBeingGrabbed) return;
        
        isBeingGrabbed = false;
        grabbingPlayer = null;
    }
    
    public bool IsBeingGrabbed()
    {
        return isBeingGrabbed;
    }
    
    public Vector2 GetVelocity()
    {
        if (chainRigidbody != null)
        {
            return chainRigidbody.linearVelocity;
        }
        return Vector2.zero;
    }
    
    public Vector2 GetPosition()
    {
        return transform.position;
    }
}
