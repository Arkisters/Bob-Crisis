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
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    public float jumpHoldTime = 0.2f;
    public float jumpHoldForce = 25f;
    
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
    private float horizontalInput;
    private bool crouchHeld;
    private bool jumpHeld;
    private bool isFacingRight = true;
    private InteractableEffects currentlyGrabbing;
    private bool carryingOnRightSide = true;
    private bool wasCrouching = false;
    private bool wasFacingRight = true;
    
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

        if (groundCheck == null)
        {
            Debug.LogWarning("No ground check transform assigned!");
        }
    }
    
    void Update()
    {
        UpdateAnimations();
        UpdateCollider();
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

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isCrouching && currentlyGrabbing == null)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        if (jumpHoldCounter > 0f && jumpHeld && currentlyGrabbing == null)
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;
        jumpHoldCounter = jumpHoldTime;
    }
    
    void HandleCrouching()
    {
        isCrouching = crouchHeld && isGrounded;
        
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
            if (currentlyGrabbing != null) return;
            
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
        
        bool isCarrying = currentlyGrabbing != null;
        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;
        
        float xValue;
        if (isCarrying)
        {
            isFacingRight = carryingOnRightSide;
            xValue = isMoving ? (carryingOnRightSide ? 1f : -1f) : (carryingOnRightSide ? 0.01f : -0.01f);
        }
        else
        {
            xValue = isMoving ? (horizontalInput > 0 ? 1f : -1f) : (isFacingRight ? 0.01f : -0.01f);
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


    void UpdateSpriteDirection()
    {
        // Only update facing direction when not carrying
        if (currentlyGrabbing == null)
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
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.transform.tag == "MovingPlatform")
        {
            transform.parent = null;
        }
    }
    
    public IEnumerator AnimatePickup(GameObject clone, InteractableEffects interactableEffects, float pickupSpeed, Quaternion startRotation)
    {
        float objectSide = clone.transform.position.x - transform.position.x;
        bool objectOnRightSide = objectSide > 0;
        carryingOnRightSide = objectOnRightSide;
        
        isFacingRight = objectOnRightSide;
        
        while (true)
        {
            float holdDistance = interactableEffects.GetHoldDistance(this, clone);
            float holdHeight = interactableEffects.GetHoldHeight(this, clone);
            float direction = objectOnRightSide ? 1f : -1f;
            Vector3 localTargetPosition = new Vector3(direction * holdDistance, holdHeight, 0);
            Vector3 worldTargetPosition = transform.TransformPoint(localTargetPosition);
            
            clone.transform.position = Vector3.MoveTowards(clone.transform.position, worldTargetPosition, pickupSpeed * Time.deltaTime);
            clone.transform.rotation = Quaternion.RotateTowards(clone.transform.rotation, Quaternion.identity, pickupSpeed * 100f * Time.deltaTime);
            
            if (Vector3.Distance(clone.transform.position, worldTargetPosition) < 0.01f)
            {
                clone.transform.rotation = Quaternion.identity;
                yield break;
            }
            
            yield return null;
        }
    }
}
