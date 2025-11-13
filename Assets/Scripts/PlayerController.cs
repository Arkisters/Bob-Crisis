using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float crouchSpeed = 2f;
    public float carryingSpeedMultiplier = 0.7f;
    public float jumpForce = 6f;
    public float airAcceleration = 50f;
    public float fastFallForce = 25f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    public float jumpHoldTime = 0.2f;
    public float jumpHoldForce = 18f;
    
    [Header("Animation")]
    public Animator animator;
    public Sprite[] walkSpritesLeft;
    public Sprite[] walkSpritesRight;
    public Sprite idleSpriteLeft;
    public Sprite idleSpriteRight;
    
    [Header("Interaction")]
    public float interactionRange = 0.75f;
    public LayerMask interactableLayer;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool isCrouching;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpHoldCounter;
    private float horizontalInput;
    private bool crouchHeld;
    private bool jumpHeld;
    private bool isFacingRight = true;
    private InteractableEffects currentlyGrabbing;
    
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_IS_JUMPING = "IsJumping";
    private const string ANIM_IS_GROUNDED = "IsGrounded";
    private const string ANIM_IS_CROUCHING = "IsCrouching";
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D component!");
        }

        if (groundCheck == null)
        {
            Debug.LogWarning("No ground check transform assigned!");
        }
    }
    
    void Update()
    {
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        CheckGroundStatus();

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        jumpBufferCounter -= Time.fixedDeltaTime;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isCrouching)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        if (jumpHoldCounter > 0f && jumpHeld)
        {
            rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
            jumpHoldCounter -= Time.fixedDeltaTime;
        }

        HandleCrouching();
        HandleMovement();
        UpdateSpriteDirection();
    }
    
    void CheckGroundStatus()
    {
        if (groundCheck == null) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }


    void Jump()
    {
        if (currentlyGrabbing != null) return;
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;
        jumpHoldCounter = jumpHoldTime;
    }
    
    void HandleCrouching()
    {
        if (crouchHeld && isGrounded)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
        
        if (crouchHeld)
        {
            rb.AddForce(Vector2.down * fastFallForce, ForceMode2D.Force);
        }
        
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
    }
    
    void HandleMovement()
    {
        float currentSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        
        if (currentlyGrabbing != null)
        {
            currentSpeed *= carryingSpeedMultiplier;
        }

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
        }
        else
        {
            float moveForce = horizontalInput * airAcceleration;
            rb.AddForce(Vector2.right * moveForce, ForceMode2D.Force);
            
            float maxAirSpeed = currentSpeed;
            if (Mathf.Abs(rb.linearVelocity.x) > maxAirSpeed)
            {
                rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxAirSpeed, rb.linearVelocity.y);
            }
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        animator.SetFloat(ANIM_SPEED, Mathf.Abs(horizontalInput));
        animator.SetBool(ANIM_IS_GROUNDED, isGrounded);
        animator.SetBool(ANIM_IS_JUMPING, !isGrounded && rb.linearVelocity.y > 0.1f);
        animator.SetBool(ANIM_IS_CROUCHING, isCrouching);
    }


    void UpdateSpriteDirection()
    {
        if (horizontalInput > 0)
        {
            isFacingRight = true;
        }
        else if (horizontalInput < 0)
        {
            isFacingRight = false;
        }
    }
    
    void TryInteract()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);
        
        foreach (Collider2D collider in hitColliders)
        {
            Vector3 directionToObject = (collider.transform.position - transform.position).normalized;
            bool objectInFrontOfPlayer = (isFacingRight && directionToObject.x > 0) || (!isFacingRight && directionToObject.x < 0);
            
            if (!objectInFrontOfPlayer) continue;
            
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                
                InteractableEffects effects = collider.GetComponent<InteractableEffects>();
                if (effects != null && effects.enableGrabbing)
                {
                    if (effects.IsBeingGrabbed())
                    {
                        currentlyGrabbing = effects;
                    }
                    else if (currentlyGrabbing == effects)
                    {
                        currentlyGrabbing = null;
                    }
                }
                break;
            }
        }
    }


    public void ReleaseGrabbedObject()
    {
        if (currentlyGrabbing != null)
        {
            currentlyGrabbing.StopGrab();
            currentlyGrabbing = null;
        }
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpHeld = true;
        }
        
        if (context.canceled)
        {
            jumpHeld = false;
        }
    }


    public void OnCrouch(InputAction.CallbackContext context)
    {
        crouchHeld = context.ReadValueAsButton();
    }


    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentlyGrabbing != null)
            {
                ReleaseGrabbedObject();
            }
            else
            {
                TryInteract();
            }
        }
    }


    public bool IsFacingRight()
    {
        return isFacingRight;
    }


    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.tag == "MovingPlatform")
        {
            transform.parent = other.transform;
            Debug.Log("Colliding");
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.transform.tag == "MovingPlatform")
        {
            transform.parent = null;
            Debug.Log("No longer colliding");
        }
    }
}
