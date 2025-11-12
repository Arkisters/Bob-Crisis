using UnityEngine;

public class InteractableEffects : MonoBehaviour, IInteractable
{
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
    public float grabForce = 5f;
    public float maxGrabDistance = 2f;
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
        
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        
        if (enableGrabbing)
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
        
        if (isBeingGrabbed && grabbingPlayer != null && objectRigidbody != null)
        {
            Vector3 playerPosition = grabbingPlayer.transform.position;
            Vector3 directionToPlayer = (playerPosition - transform.position);
            float distanceToPlayer = directionToPlayer.magnitude;
            
            if (distanceToPlayer > 0.5f)
            {
                Vector3 forceDirection = directionToPlayer.normalized;
                objectRigidbody.AddForce(forceDirection * grabForce, ForceMode2D.Force);
            }
            
            if (distanceToPlayer > maxGrabDistance)
            {
                Debug.Log($"[GRAB] {gameObject.name} too far from player, releasing grab");
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


    public void StartGrab(PlayerController player)
    {
        if (!enableGrabbing)
        {
            Debug.Log($"[GRAB] Cannot grab {gameObject.name} - grabbing not enabled");
            return;
        }
        
        if (isBeingGrabbed)
        {
            Debug.Log($"[GRAB] Cannot grab {gameObject.name} - already being grabbed");
            return;
        }
        
        if (objectRigidbody == null)
        {
            Debug.LogError($"[GRAB] Cannot grab {gameObject.name} - missing Rigidbody2D!");
            return;
        }
        
        Debug.Log($"[GRAB] Starting grab on {gameObject.name} by player");
        isBeingGrabbed = true;
        grabbingPlayer = player;
        
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
    }


    public void StopGrab()
    {
        if (!enableGrabbing || !isBeingGrabbed) return;
        
        Debug.Log($"[GRAB] Stopping grab on {gameObject.name}");
        isBeingGrabbed = false;
        grabbingPlayer = null;
    }


    public bool IsBeingGrabbed()
    {
        return isBeingGrabbed;
    }


    public void Interact(PlayerController player)
    {
        Debug.Log($"[INTERACT] {gameObject.name} Interact() called by player");
        
        if (enableColorChange)
        {
            Debug.Log($"[INTERACT] Toggling color on {gameObject.name}");
            ToggleColor();
        }
        
        if (enableObjectToggle)
        {
            Debug.Log($"[INTERACT] Toggling object on {gameObject.name}");
            ToggleObject();
        }
        
        if (enableMovement)
        {
            Debug.Log($"[INTERACT] Toggling movement on {gameObject.name}");
            ToggleMove();
        }
        
        if (enableObjectActivation)
        {
            Debug.Log($"[INTERACT] Activating objects from {gameObject.name}");
            ActivateObjects();
        }
        
        if (enableGrabbing) 
        {
            if (isBeingGrabbed)
            {
                Debug.Log($"[INTERACT] Releasing grab on {gameObject.name}");
                StopGrab();
            }
            else
            {
                Debug.Log($"[INTERACT] Attempting to grab {gameObject.name}");
                StartGrab(player);
            }
        }
    }
}
