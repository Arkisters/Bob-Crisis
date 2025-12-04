using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float crouchSpeed = 2f;
    public float carryingSpeedMultiplier = 0.7f;
    public float jumpForce = 8f;
    public float airAcceleration = 50f;
    public float fastFallForce = 15f;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.025f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    public float jumpHoldTime = 0.2f;
    public float jumpHoldForce = 25f;
    public float carryingJumpMultiplier = 0.75f;
    
    [Header("Interaction Settings")]
    public float interactionBufferTime = 0.15f;
    
    [Header("Climbing Settings")]
    public float climbSpeed = 4f;
    public float climbingLinearDamping = 15f;
    private float normalLinearDamping;
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Interaction")]
    public float interactionRange = 0.75f;
    public LayerMask interactableLayer;
    
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded;
    private bool isCrouching;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpHoldCounter;
    private float interactionBufferCounter;
    private float horizontalInput;
    private bool crouchHeld;
    private bool jumpHeld;
    private bool isFacingRight = true;
    private InteractableEffects currentlyGrabbing;
    private bool wasCrouching = false;
    private bool wasFacingRight = true;
    private MovingPlatforms currentPlatform;
    private Vector3 platformVelocity;
    private bool isOnLadder;
    private bool isClimbing;
    
    private const string ANIM_X = "x";
    private const string ANIM_IS_CROUCHING = "isCrouching";
    private const string ANIM_IS_CARRYING = "isCarrying";
    
    // Collider constants
    private static readonly Vector2 NORMAL_SIZE = new Vector2(0.17f, 0.44f);
    private static readonly Vector2 CROUCH_SIZE = new Vector2(0.17f, 0.39f);
    private const float OFFSET_Y_NORMAL = -0.01f;
    private const float OFFSET_Y_CROUCH_RIGHT = -0.035f;
    private const float OFFSET_Y_CROUCH_LEFT = -0.035f;
    private const float OFFSET_X_RIGHT = -0.005f;
    private const float OFFSET_X_LEFT = 0.015f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D component!");
        }
        else
        {
            normalLinearDamping = rb.linearDamping;
        }
    }
    
    void Update()
    {
        UpdateFacingDirection();
        UpdateAnimations();
        UpdateCollider();
    }

    void FixedUpdate()
    {
        CheckGroundStatus();
        UpdateClimbingState();

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        jumpBufferCounter -= Time.fixedDeltaTime;
        interactionBufferCounter -= Time.fixedDeltaTime;

        // Can't jump when on a ladder - instead start climbing
        if (jumpBufferCounter > 0f && isOnLadder)
        {
            if (!isClimbing)
            {
                isClimbing = true;
                rb.gravityScale = 0f;
                rb.linearDamping = climbingLinearDamping;
            }
            jumpBufferCounter = 0f;
        }
        
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isCrouching && !isOnLadder)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // Always count down jump hold timer
        if (jumpHoldCounter > 0f)
        {
            jumpHoldCounter -= Time.fixedDeltaTime;
            
            // Apply jump hold force (reduced when carrying)
            if (jumpHeld && rb.linearVelocity.y > 0f)
            {
                float holdForce = jumpHoldForce;
                if (currentlyGrabbing != null)
                {
                    holdForce *= carryingJumpMultiplier;
                }
                rb.AddForce(Vector2.up * holdForce, ForceMode2D.Force);
            }
        }
        
        // Handle interaction buffering - try to interact if buffer is active
        if (interactionBufferCounter > 0f)
        {
            bool interacted = TryInteract();
            if (interacted)
            {
                interactionBufferCounter = 0f;
            }
        }
        
        // Check if grabbed object is still in range
        if (currentlyGrabbing != null)
        {
            if (!IsObjectInRange(currentlyGrabbing.gameObject))
            {
                currentlyGrabbing.StopGrab();
                currentlyGrabbing = null;
            }
        }

        HandleCrouching();
        HandleMovement();
    }
    
    void UpdateFacingDirection()
    {
        // Only update facing direction when not carrying
        if (currentlyGrabbing == null && Mathf.Abs(horizontalInput) > 0.01f)
        {
            isFacingRight = horizontalInput > 0;
        }
    }
    
    void CheckGroundStatus()
    {
        float scaleY = transform.lossyScale.y;
        float scaleX = transform.lossyScale.x;
        
        float bottomY = transform.position.y + (boxCollider.offset.y * scaleY) - (boxCollider.size.y * scaleY / 2f);
        float halfWidth = (boxCollider.size.x * scaleX) / 2f;
        float offsetX = boxCollider.offset.x * scaleX;
        
        Vector2 leftCorner = new Vector2(transform.position.x + offsetX - halfWidth, bottomY);
        Vector2 rightCorner = new Vector2(transform.position.x + offsetX + halfWidth, bottomY);
        
        RaycastHit2D leftHit = Physics2D.Raycast(leftCorner, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightCorner, Vector2.down, groundCheckDistance, groundLayer);
        
        isGrounded = leftHit || rightHit;
        
        // Check if standing on a moving platform
        MovingPlatforms newPlatform = null;
        if (leftHit.collider != null)
        {
            newPlatform = leftHit.collider.GetComponent<MovingPlatforms>();
        }
        if (newPlatform == null && rightHit.collider != null)
        {
            newPlatform = rightHit.collider.GetComponent<MovingPlatforms>();
        }
        
        // Update platform and velocity
        if (newPlatform != null && isGrounded)
        {
            currentPlatform = newPlatform;
            platformVelocity = currentPlatform.GetVelocity();
        }
        else
        {
            currentPlatform = null;
            platformVelocity = Vector3.zero;
        }
    }


    void Jump()
    {
        // Transfer platform velocity on jump
        Vector2 finalVelocity = rb.linearVelocity;
        if (currentPlatform != null)
        {
            finalVelocity.x += platformVelocity.x;
            finalVelocity.y = platformVelocity.y;
        }
        else
        {
            finalVelocity.y = 0f;
        }
        
        rb.linearVelocity = finalVelocity;
        
        // Reduce jump force when carrying
        float actualJumpForce = jumpForce;
        if (currentlyGrabbing != null)
        {
            actualJumpForce *= carryingJumpMultiplier;
        }
        
        rb.AddForce(Vector2.up * actualJumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;
        jumpHoldCounter = jumpHoldTime;
        
        // Clear platform reference when jumping off
        currentPlatform = null;
        platformVelocity = Vector3.zero;
    }
    
    void HandleCrouching()
    {
        isCrouching = crouchHeld && isGrounded && !isClimbing;
        
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
        // Handle climbing movement separately
        if (isClimbing)
        {
            HandleClimbingMovement();
            return;
        }
        
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
            // Apply player movement
            float playerVelocityX = horizontalInput * currentSpeed;
            
            // Add platform velocity if on a platform
            if (currentPlatform != null)
            {
                rb.linearVelocity = new Vector2(playerVelocityX + platformVelocity.x, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(playerVelocityX, rb.linearVelocity.y);
            }
        }
        else
        {
            float airControl = crouchHeld ? airAcceleration * 0.3f : airAcceleration;
            
            // Reduce air control when carrying
            if (currentlyGrabbing != null)
            {
                airControl *= carryingSpeedMultiplier;
            }
            
            float moveForce = horizontalInput * airControl;
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
        
        bool isCarrying = currentlyGrabbing != null;
        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;
        
        float xValue;
        if (isMoving)
        {
            xValue = isFacingRight ? 1f : -1f;
        }
        else
        {
            xValue = isFacingRight ? 0.01f : -0.01f;
        }
        
        animator.SetFloat(ANIM_X, xValue);
        animator.SetBool(ANIM_IS_CROUCHING, crouchHeld);
        animator.SetBool(ANIM_IS_CARRYING, isCarrying);
        
        float animSpeed = 1f;
        if (isMoving)
        {
            float currentSpeed = isCarrying ? walkSpeed * carryingSpeedMultiplier : walkSpeed;
            float velocityRatio = Mathf.Abs(rb.linearVelocity.x) / currentSpeed;
            animSpeed = Mathf.Clamp(velocityRatio, 0.5f, 1.5f);
        }
        else if (isCarrying)
        {
            animSpeed = carryingSpeedMultiplier;
        }
        
        animator.speed = animSpeed;
    }

    void UpdateCollider()
    {
        if (boxCollider == null) return;
        
        // Only update if crouch or facing direction changed
        if (wasCrouching != isCrouching || wasFacingRight != isFacingRight)
        {
            float offsetX = isFacingRight ? OFFSET_X_RIGHT : OFFSET_X_LEFT;
            
            if (isCrouching)
            {
                boxCollider.size = CROUCH_SIZE;
                float offsetY = isFacingRight ? OFFSET_Y_CROUCH_RIGHT : OFFSET_Y_CROUCH_LEFT;
                boxCollider.offset = new Vector2(offsetX, offsetY);
            }
            else
            {
                boxCollider.size = NORMAL_SIZE;
                boxCollider.offset = new Vector2(offsetX, OFFSET_Y_NORMAL);
            }
            
            wasCrouching = isCrouching;
            wasFacingRight = isFacingRight;
        }
    }


    
    bool TryInteract()
    {
        if (boxCollider == null) return false;
        
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 castOrigin = (Vector2)transform.position;
        Vector2 boxSize = new Vector2(0.1f, boxCollider.bounds.size.y * 0.8f);
        
        RaycastHit2D[] hits = Physics2D.BoxCastAll(castOrigin, boxSize, 0f, castDirection, interactionRange, interactableLayer);
        
        if (hits.Length == 0) return false;
        
        // Sort by distance to get closest object
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        foreach (RaycastHit2D hit in hits)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                return true; // Successfully interacted
            }
        }
        
        return false;
    }
    
    bool IsObjectInRange(GameObject obj)
    {
        if (boxCollider == null || obj == null) return false;
        
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 castOrigin = (Vector2)transform.position;
        Vector2 boxSize = new Vector2(0.1f, boxCollider.bounds.size.y * 0.8f);
        
        RaycastHit2D[] hits = Physics2D.BoxCastAll(castOrigin, boxSize, 0f, castDirection, interactionRange, interactableLayer);
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == obj)
            {
                return true;
            }
        }
        
        return false;
    }


    public void SetGrabbedObject(InteractableEffects obj)
    {
        currentlyGrabbing = obj;
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
            if (jumpHoldCounter > 0f) return;
            
            if (currentlyGrabbing != null)
            {
                currentlyGrabbing.StopGrab();
                currentlyGrabbing = null;
            }
            else
            {
                // Try to interact immediately
                bool interacted = TryInteract();
                
                // If couldn't interact, start buffer to try again
                if (!interacted)
                {
                    interactionBufferCounter = interactionBufferTime;
                }
            }
        }
    }


    public bool IsFacingRight()
    {
        return isFacingRight;
    }
    
    void UpdateClimbingState()
    {
        // Deactivate climbing only if grounded AND moving down (crouching)
        if (isClimbing && isGrounded && crouchHeld)
        {
            isClimbing = false;
            rb.gravityScale = 2f;
            rb.linearDamping = normalLinearDamping;
        }
        
        // Auto-start climbing if on ladder and airborne (falling into ladder)
        if (isOnLadder && !isGrounded && !isClimbing)
        {
            isClimbing = true;
            rb.gravityScale = 0f;
            rb.linearDamping = climbingLinearDamping;
        }
    }
    
    void HandleClimbingMovement()
    {
        // Handle climbing up with jump (hold supported)
        if (jumpHeld)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, climbSpeed);
        }
        // Handle climbing down with crouch (hold supported)
        else if (crouchHeld)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -climbSpeed);
        }
        else
        {
            // Not pressing up or down, stop vertical movement
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        
        // Allow horizontal movement to get off ladder (but dampened by high linear damping)
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            float moveForce = horizontalInput * airAcceleration * 0.5f;
            rb.AddForce(Vector2.right * moveForce, ForceMode2D.Force);
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = true;
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = false;
            isClimbing = false;
            rb.gravityScale = 2f;
            rb.linearDamping = normalLinearDamping;
        }
    }


    void OnDrawGizmosSelected()
    {
        if (boxCollider != null)
        {
            float scaleY = transform.lossyScale.y;
            float scaleX = transform.lossyScale.x;
            
            float bottomY = transform.position.y + (boxCollider.offset.y * scaleY) - (boxCollider.size.y * scaleY / 2f);
            float halfWidth = (boxCollider.size.x * scaleX) / 2f;
            float offsetX = boxCollider.offset.x * scaleX;
            
            Vector2 leftCorner = new Vector2(transform.position.x + offsetX - halfWidth, bottomY);
            Vector2 rightCorner = new Vector2(transform.position.x + offsetX + halfWidth, bottomY);
            
            Vector2 leftEnd = leftCorner + Vector2.down * groundCheckDistance;
            Vector2 rightEnd = rightCorner + Vector2.down * groundCheckDistance;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(leftCorner, leftEnd);
            Gizmos.DrawLine(rightCorner, rightEnd);
            Gizmos.DrawWireSphere(leftCorner, 0.02f);
            Gizmos.DrawWireSphere(rightCorner, 0.02f);
            Gizmos.DrawWireSphere(leftEnd, 0.02f);
            Gizmos.DrawWireSphere(rightEnd, 0.02f);
        }
        
        // Interaction box cast visualization
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 castOrigin = (Vector2)transform.position;
        Vector2 boxSize = new Vector2(0.1f, boxCollider != null ? boxCollider.bounds.size.y * 0.8f : 1f);
        
        Gizmos.color = Color.yellow;
        Vector3 boxCenter = castOrigin + castDirection * (interactionRange * 0.5f);
        Gizmos.DrawWireCube(boxCenter, new Vector3(interactionRange, boxSize.y, 0));
    }
}
