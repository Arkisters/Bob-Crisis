using System.Collections;
using UnityEngine;

public class InteractableEffects : MonoBehaviour, IInteractable
{
    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    [Header("Color Change")]
    public bool enableColorChange = false;
    public Color targetColor = Color.green;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    [Header("Object Toggle")]
    public bool enableObjectToggle = false;
    public GameObject objectToToggle;
    
    [Header("Object Movement")]
    public bool enableMovement = false;
    public Vector3 moveOffset = new Vector3(0, 2, 0);
    public float moveDuration = 1f;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveTimer = 0f;
    
    [Header("Object Activation")]
    public bool enableObjectActivation = false;
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    
    [Header("Grab and Carry")]
    public bool enableGrabbing = false;
    public float groundClearance = 0.3f;
    public float holdForce = 250f;
    public float rotationForce = 250f;
    public float maxHoldDistance = 1f;
    public PhysicsMaterial2D zeroFrictionMaterial;
    private bool isBeingGrabbed = false;
    private PlayerController grabbingPlayer;
    private Rigidbody2D objectRigidbody;
    private Collider2D objectCollider;
    private PhysicsMaterial2D originalMaterial;

    [Header("Timed Toggle")]
    public bool enableTimedToggle = false;
    public float activationTimer;
    public GameObject timedObjectsToToggle;

    [Header("Animate and Kill")]
    public bool enableAnimateAndKill = false;
    public GameObject objectToAnimate;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectRigidbody = GetComponent<Rigidbody2D>();
        objectCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        originalPosition = transform.position;
        targetPosition = originalPosition + moveOffset;
        
        if (enableGrabbing && objectRigidbody == null)
        {
            Debug.LogWarning($"InteractableEffects on {gameObject.name} has grab enabled but no Rigidbody2D component!");
        }
    }


    void FixedUpdate()
    {
        if (isMoving)
        {
            moveTimer += Time.fixedDeltaTime;
            float progress = moveTimer / moveDuration;
            transform.position = Vector3.Lerp(originalPosition, targetPosition, progress);
            
            if (progress >= 1f)
            {
                isMoving = false;
            }
        }
        
        // Physics-based holding when grabbed
        if (isBeingGrabbed && grabbingPlayer != null && objectRigidbody != null)
        {
            // Calculate target position in world space
            Vector3 targetWorldPosition = GetTargetHoldPosition();
            
            // Apply force to move object toward target position
            Vector2 direction = (targetWorldPosition - transform.position);
            float distance = direction.magnitude;
            
            if (distance > 0.01f)
            {
                Vector2 force = direction.normalized * holdForce * distance;
                objectRigidbody.AddForce(force, ForceMode2D.Force);
            }
            
            // Snap rotation to nearest 90-degree angle
            float currentRotation = transform.eulerAngles.z;
            float targetRotation = Mathf.Round(currentRotation / 90f) * 90f;
            float rotationDifference = Mathf.DeltaAngle(currentRotation, targetRotation);
            
            if (Mathf.Abs(rotationDifference) > 0.1f)
            {
                float torque = rotationDifference * rotationForce * Mathf.Deg2Rad;
                objectRigidbody.AddTorque(torque, ForceMode2D.Force);
            }
            
            // Dampen velocities to prevent oscillation
            objectRigidbody.linearVelocity *= 0.9f;
            objectRigidbody.angularVelocity *= 0.9f;
        }
    }


    public void ChangeColor()
    {
        if (!enableColorChange || spriteRenderer == null) return;
        spriteRenderer.color = targetColor;
    }


    public void ResetColor()
    {
        if (!enableColorChange || spriteRenderer == null) return;
        spriteRenderer.color = originalColor;
    }


    public void ToggleColor()
    {
        if (!enableColorChange || spriteRenderer == null) return;
        
        if (spriteRenderer.color == originalColor)
        {
            spriteRenderer.color = targetColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }


    public void ToggleObject()
    {
        if (!enableObjectToggle || objectToToggle == null) return;
        objectToToggle.SetActive(!objectToToggle.activeSelf);
    }


    public void EnableObject()
    {
        if (!enableObjectToggle || objectToToggle == null) return;
        objectToToggle.SetActive(true);
    }


    public void DisableObject()
    {
        if (!enableObjectToggle || objectToToggle == null) return;
        objectToToggle.SetActive(false);
    }


    public void MoveToTarget()
    {
        if (!enableMovement) return;
        
        originalPosition = transform.position;
        targetPosition = originalPosition + moveOffset;
        moveTimer = 0f;
        isMoving = true;
    }


    public void MoveToOriginal()
    {
        if (!enableMovement) return;
        
        targetPosition = transform.position;
        originalPosition = targetPosition - moveOffset;
        moveTimer = 0f;
        isMoving = true;
    }


    public void ToggleMove()
    {
        if (!enableMovement) return;
        
        if (Vector3.Distance(transform.position, originalPosition) < 0.1f)
        {
            MoveToTarget();
        }
        else
        {
            MoveToOriginal();
        }
    }


    public void ActivateObjects()
    {
        if (!enableObjectActivation) return;
        
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }


    public bool StartGrab(PlayerController player)
    {
        if (!enableGrabbing || isBeingGrabbed || objectRigidbody == null) return false;
        
        isBeingGrabbed = true;
        audioManager.PlaySFX(audioManager.BlokPickup);
        grabbingPlayer = player;
        
        // Store original material and apply zero friction material
        if (objectCollider != null)
        {
            if (objectCollider is BoxCollider2D boxCol)
            {
                originalMaterial = boxCol.sharedMaterial;
                if (zeroFrictionMaterial != null)
                {
                    boxCol.sharedMaterial = zeroFrictionMaterial;
                }
            }
            else if (objectCollider is CircleCollider2D circleCol)
            {
                originalMaterial = circleCol.sharedMaterial;
                if (zeroFrictionMaterial != null)
                {
                    circleCol.sharedMaterial = zeroFrictionMaterial;
                }
            }
        }
        
        return true;
    }
    
    Vector3 GetTargetHoldPosition()
    {
        if (grabbingPlayer == null) return transform.position;
        
        bool playerFacingRight = grabbingPlayer.IsFacingRight();
        float direction = playerFacingRight ? 1f : -1f;
        
        // Calculate hold distance based on player and object sizes
        float playerWidth = 0f;
        Collider2D playerCollider = grabbingPlayer.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerWidth = playerCollider.bounds.size.x / 2f;
        }
        
        float objectWidth = 0f;
        if (objectCollider != null)
        {
            objectWidth = objectCollider.bounds.size.x / 2f;
        }
        
        float holdDistance = playerWidth + objectWidth;
        
        // Calculate hold height
        float playerBottomY = 0f;
        if (playerCollider != null)
        {
            playerBottomY = playerCollider.bounds.min.y;
        }
        
        float objectHeight = 0f;
        if (objectCollider != null)
        {
            objectHeight = objectCollider.bounds.size.y / 2f;
        }
        
        float targetX = grabbingPlayer.transform.position.x + (direction * holdDistance);
        float targetY = playerBottomY + objectHeight + groundClearance;
        
        return new Vector3(targetX, targetY, grabbingPlayer.transform.position.z);
    }


    public void StopGrab()
    {
        if (!enableGrabbing || !isBeingGrabbed) return;
        
        isBeingGrabbed = false;
        audioManager.PlaySFX(audioManager.BlokPutDown);
        grabbingPlayer = null;
        
        // Restore original physics material
        if (objectCollider != null)
        {
            if (objectCollider is BoxCollider2D boxCol)
            {
                boxCol.sharedMaterial = originalMaterial;
            }
            else if (objectCollider is CircleCollider2D circleCol)
            {
                circleCol.sharedMaterial = originalMaterial;
            }
        }
    }


    public bool IsBeingGrabbed()
    {
        return isBeingGrabbed;
    }


    public void Interact(PlayerController player)
    {
        if (enableColorChange) ToggleColor();
        if (enableObjectToggle) ToggleObject();
        if (enableMovement) ToggleMove();
        if (enableObjectActivation) ActivateObjects();
        if (enableTimedToggle) TimedToggle();
        audioManager.PlaySFX(audioManager.ButtonPress);
        if (enableAnimateAndKill) AnimateAndKill();

        if (enableGrabbing) 
        {
            if (StartGrab(player))
            {
                player.SetGrabbedObject(this);
            }
        }
    }

    public void TimedToggle()
    {
        if (!enableTimedToggle || timedObjectsToToggle == null) return;
        StartCoroutine(DisabledObject(1f));
    }

    IEnumerator DisabledObject(float duration)
    {
        timedObjectsToToggle.SetActive(!timedObjectsToToggle.activeSelf);
        yield return new WaitForSeconds(activationTimer);
        timedObjectsToToggle.SetActive(!timedObjectsToToggle.activeSelf);
    }

    public void AnimateAndKill()
    {
        if (!enableAnimateAndKill || objectToAnimate != null)
        {
            Animator animator = objectToAnimate.GetComponent<Animator>();
            animator.SetBool("explode", true);
            Destroy(gameObject, 1f); // Adjust delay as needed to match animation length
        }
    }
}
