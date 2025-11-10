using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2f;
    public float jumpForce = 10f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.2f;
    
    [Header("Animation")]
    public Animator animator;
    public Sprite[] walkSpritesLeft;
    public Sprite[] walkSpritesRight;
    public Sprite idleSpriteLeft;
    public Sprite idleSpriteRight;
    
    [Header("Interaction")]
    public float interactionRange = 1.5f;
    public LayerMask interactableLayer;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool isCrouching;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float horizontalInput;
    private bool crouchHeld;
    private bool isFacingRight = true;
    
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

        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
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
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                break;
            }
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
            TryInteract();
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
}
