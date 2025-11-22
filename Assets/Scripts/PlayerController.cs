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

        // Always count down jump hold timer
        if (jumpHoldCounter > 0f)
        {
            jumpHoldCounter -= Time.fixedDeltaTime;
            
            // Apply jump hold force only when conditions are met
            if (jumpHeld && currentlyGrabbing == null && rb.linearVelocity.y > 0f)
            {
                rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
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
        
        bool leftHit = Physics2D.Raycast(leftCorner, Vector2.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics2D.Raycast(rightCorner, Vector2.down, groundCheckDistance, groundLayer);
        
        isGrounded = leftHit || rightHit;
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
            
            float airControl = crouchHeld ? airAcceleration * 0.3f : airAcceleration;
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


    
    void TryInteract()
    {
        if (boxCollider == null) return;
        
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 castOrigin = (Vector2)transform.position;
        Vector2 boxSize = new Vector2(0.1f, boxCollider.bounds.size.y * 0.8f);
        
        RaycastHit2D[] hits = Physics2D.BoxCastAll(castOrigin, boxSize, 0f, castDirection, interactionRange, interactableLayer);
        
        if (hits.Length == 0) return;
        
        // Sort by distance to get closest object
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        foreach (RaycastHit2D hit in hits)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                break;
            }
        }
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
        // Determine which side the object is on and lock facing direction
        bool objectOnRightSide = clone.transform.position.x > transform.position.x;
        isFacingRight = objectOnRightSide;
        
        while (true)
        {
            // Recalculate which side based on current position each frame
            bool currentlyOnRight = clone.transform.position.x > transform.position.x;
            float direction = currentlyOnRight ? 1f : -1f;
            
            float holdDistance = interactableEffects.GetHoldDistance(this, clone);
            float holdHeight = interactableEffects.GetHoldHeight(this, clone);
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
