using UnityEngine;

public class InteractableEffects : MonoBehaviour, IInteractable
{
    [Header("Color Effects")]
    public bool enableColorChange = false;
    public Color targetColor = Color.green;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    [Header("Toggle Effects")]
    public bool enableToggle = false;
    public GameObject objectToToggle;
    
    [Header("Movement Effects")]
    public bool enableMovement = false;
    public Vector3 moveOffset = new Vector3(0, 2, 0);
    public float moveDuration = 1f;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveTimer = 0f;
    
    [Header("Activation Effects")]
    public bool enableActivation = false;
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    
    [Header("Grab Effects")]
    public bool enableGrab = false;
    public float grabForce = 5f; // Force applied to pull box toward player
    public float maxGrabDistance = 2f; // Maximum distance to maintain grab
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private bool isBeingGrabbed = false;
    private PlayerController grabbingPlayer;
    private Rigidbody2D objectRigidbody;
    private Collider2D objectCollider;


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
        
        // Store original parent and local position for grab functionality
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        
        // Warn if missing components for grab functionality
        if (enableGrab)
        {
            if (objectRigidbody == null)
                Debug.LogWarning($"InteractableEffects on {gameObject.name} has grab enabled but no Rigidbody2D component!");
            if (objectCollider == null)
                Debug.LogWarning($"InteractableEffects on {gameObject.name} has grab enabled but no Collider2D component!");
        }
    }


    void Update()
    {
        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            float progress = moveTimer / moveDuration;
            transform.position = Vector3.Lerp(originalPosition, targetPosition, progress);
            
            if (progress >= 1f)
            {
                isMoving = false;
            }
        }
        
        // Handle grab with simple force toward player
        if (isBeingGrabbed && grabbingPlayer != null && objectRigidbody != null)
        {
            Vector3 playerPosition = grabbingPlayer.transform.position;
            Vector3 directionToPlayer = (playerPosition - transform.position);
            float distanceToPlayer = directionToPlayer.magnitude;
            
            // Only apply force if not too close (to prevent jittering when touching)
            if (distanceToPlayer > 0.5f)
            {
                Vector3 forceDirection = directionToPlayer.normalized;
                objectRigidbody.AddForce(forceDirection * grabForce, ForceMode2D.Force);
            }
            
            // Break grab if too far away
            if (distanceToPlayer > maxGrabDistance)
            {
                StopGrab();
                if (grabbingPlayer != null)
                {
                    grabbingPlayer.ReleaseGrabbedObject();
                }
            }
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
        if (!enableToggle || objectToToggle == null) return;
        objectToToggle.SetActive(!objectToToggle.activeSelf);
    }


    public void EnableObject()
    {
        if (!enableToggle || objectToToggle == null) return;
        objectToToggle.SetActive(true);
    }


    public void DisableObject()
    {
        if (!enableToggle || objectToToggle == null) return;
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
        if (!enableActivation) return;
        
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


    public void StartGrab(PlayerController player)
    {
        if (!enableGrab || isBeingGrabbed || objectRigidbody == null) return;
        
        isBeingGrabbed = true;
        grabbingPlayer = player;
        
        // Store original parent and position
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
    }


    public void StopGrab()
    {
        if (!enableGrab || !isBeingGrabbed) return;
        
        isBeingGrabbed = false;
        grabbingPlayer = null;
    }


    public bool IsBeingGrabbed()
    {
        return isBeingGrabbed;
    }


    public void Interact(PlayerController player)
    {
        if (enableColorChange) ToggleColor();
        if (enableToggle) ToggleObject();
        if (enableMovement) ToggleMove();
        if (enableActivation) ActivateObjects();
        if (enableGrab) 
        {
            if (isBeingGrabbed)
            {
                StopGrab();
            }
            else
            {
                StartGrab(player);
            }
        }
    }
}
