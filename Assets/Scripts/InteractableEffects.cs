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
    public float groundClearance = 0.3f;
    public float pickupSpeed = 5f;
    public PhysicsMaterial2D zeroFrictionMaterial;
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private bool isBeingGrabbed = false;
    private PlayerController grabbingPlayer;
    private Rigidbody2D objectRigidbody;
    private GameObject carriedClone;
    private bool isAnimatingPickup = false;
    private Quaternion pickupStartRotation;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectRigidbody = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        originalPosition = transform.position;
        targetPosition = originalPosition + moveOffset;
        
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        
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
        
        if (isAnimatingPickup && carriedClone != null && grabbingPlayer != null)
        {
            float holdDistance = CalculateHoldDistance(grabbingPlayer);
            float holdHeight = CalculateHoldHeight(grabbingPlayer);
            float direction = grabbingPlayer.IsFacingRight() ? 1f : -1f;
            Vector3 localTargetPosition = new Vector3(direction * holdDistance, holdHeight, 0);
            Vector3 worldTargetPosition = grabbingPlayer.transform.TransformPoint(localTargetPosition);
            
            carriedClone.transform.position = Vector3.MoveTowards(carriedClone.transform.position, worldTargetPosition, pickupSpeed * Time.fixedDeltaTime);
            carriedClone.transform.rotation = Quaternion.RotateTowards(carriedClone.transform.rotation, Quaternion.identity, pickupSpeed * 100f * Time.fixedDeltaTime);
            
            if (Vector3.Distance(carriedClone.transform.position, worldTargetPosition) < 0.01f)
            {
                isAnimatingPickup = false;
                carriedClone.transform.rotation = Quaternion.identity;
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
        if (!enableGrabbing || isBeingGrabbed || objectRigidbody == null) return;
        
        isBeingGrabbed = true;
        grabbingPlayer = player;
        
        carriedClone = new GameObject(gameObject.name + "_Carried");
        carriedClone.transform.SetParent(player.transform);
        
        pickupStartRotation = transform.rotation;
        
        SpriteRenderer cloneSpriteRenderer = carriedClone.AddComponent<SpriteRenderer>();
        SpriteRenderer originalSpriteRenderer = GetComponent<SpriteRenderer>();
        if (originalSpriteRenderer != null)
        {
            cloneSpriteRenderer.sprite = originalSpriteRenderer.sprite;
            cloneSpriteRenderer.color = originalSpriteRenderer.color;
            cloneSpriteRenderer.sortingLayerName = originalSpriteRenderer.sortingLayerName;
            cloneSpriteRenderer.sortingOrder = originalSpriteRenderer.sortingOrder;
        }
        
        Vector3 worldScale = transform.lossyScale;
        Vector3 parentScale = player.transform.lossyScale;
        carriedClone.transform.localScale = new Vector3(
            worldScale.x / parentScale.x,
            worldScale.y / parentScale.y,
            worldScale.z / parentScale.z
        );
        
        BoxCollider2D originalBoxCollider = GetComponent<BoxCollider2D>();
        if (originalBoxCollider != null)
        {
            BoxCollider2D cloneCollider = carriedClone.AddComponent<BoxCollider2D>();
            cloneCollider.size = originalBoxCollider.size;
            cloneCollider.offset = originalBoxCollider.offset;
            cloneCollider.sharedMaterial = zeroFrictionMaterial;
        }
        
        CircleCollider2D originalCircleCollider = GetComponent<CircleCollider2D>();
        if (originalCircleCollider != null)
        {
            CircleCollider2D cloneCollider = carriedClone.AddComponent<CircleCollider2D>();
            cloneCollider.radius = originalCircleCollider.radius;
            cloneCollider.offset = originalCircleCollider.offset;
            cloneCollider.sharedMaterial = zeroFrictionMaterial;
        }
        
        carriedClone.transform.position = transform.position;
        carriedClone.transform.rotation = pickupStartRotation;
        
        isAnimatingPickup = true;
        
        gameObject.SetActive(false);
    }
    
    float CalculateHoldDistance(PlayerController player)
    {
        float playerWidth = 0f;
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerWidth = playerCollider.bounds.size.x / 2f;
        }
        
        float objectWidth = 0f;
        Collider2D objectCollider = GetComponent<Collider2D>();
        if (objectCollider != null)
        {
            objectWidth = objectCollider.bounds.size.x / 2f;
        }
        
        return (playerWidth + objectWidth) / player.transform.lossyScale.x;
    }
    
    float CalculateHoldHeight(PlayerController player)
    {
        float playerBottomY = 0f;
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerBottomY = playerCollider.bounds.min.y - player.transform.position.y;
        }
        
        float objectHeight = 0f;
        Collider2D objectCollider = GetComponent<Collider2D>();
        if (objectCollider != null)
        {
            objectHeight = objectCollider.bounds.size.y / 2f;
        }
        
        float localGroundY = playerBottomY / player.transform.lossyScale.y;
        float heightOffset = (objectHeight + groundClearance) / player.transform.lossyScale.y;
        
        return localGroundY + heightOffset;
    }


    public void StopGrab()
    {
        if (!enableGrabbing || !isBeingGrabbed) return;
        
        isBeingGrabbed = false;
        isAnimatingPickup = false;
        grabbingPlayer = null;
        
        if (carriedClone != null)
        {
            transform.position = carriedClone.transform.position;
            transform.rotation = carriedClone.transform.rotation;
            Destroy(carriedClone);
            carriedClone = null;
        }
        
        gameObject.SetActive(true);
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
        
        if (enableGrabbing) 
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
