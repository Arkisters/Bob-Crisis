using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    
    [Header("Level Bounds (in camera units)")]
    public int boundsWidth = 2;
    public int boundsHeight = 2;
    
    [Header("Vertical Follow Settings")]
    [Tooltip("Smooth speed for vertical camera movement")]
    public float verticalSmoothSpeed = 5f;
    
    [Header("Horizontal Transition Settings")]
    [Tooltip("Speed for horizontal camera transitions")]
    public float horizontalTransitionSpeed = 2f;
    [Tooltip("Amount of overshoot (0 = no overshoot, 0.1 = slight overshoot)")]
    public float overshootAmount = 0.1f;
    
    private Camera cam;
    private float cameraWidth;
    private float cameraHeight;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private float minX, maxX, minY, maxY;
    private float targetHorizontalX;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Calculate camera dimensions in world units
        cameraHeight = cam.orthographicSize * 2f;
        cameraWidth = cameraHeight * cam.aspect;
        
        // Calculate bounds using camera's starting position as bottom-left
        CalculateBounds();
        
        // Initialize target position to current position (don't move camera)
        targetPosition = transform.position;
        targetHorizontalX = transform.position.x;
    }
    
    void CalculateBounds()
    {
        // Camera's current position IS the bottom-left valid camera position
        // So bounds extend from here
        Vector3 startPos = transform.position;
        
        minX = startPos.x;
        maxX = startPos.x + cameraWidth * (boundsWidth - 1);
        minY = startPos.y;
        maxY = startPos.y + cameraHeight * (boundsHeight - 1);
    }
    
    void LateUpdate()
    {
        if (player == null) return;
        
        HandleHorizontalMovement();
        HandleVerticalMovement();
        ApplyBounds();
        
        // Apply position (keep Z unchanged)
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
    }
    
    void HandleHorizontalMovement()
    {
        float playerX = player.position.x;
        float cameraX = targetHorizontalX;
        float halfWidth = cameraWidth / 2f;
        
        // Check if player went outside camera view
        if (playerX > cameraX + halfWidth)
        {
            // Player went right - move camera to next section
            targetHorizontalX += cameraWidth;
        }
        else if (playerX < cameraX - halfWidth)
        {
            // Player went left - move camera to previous section
            targetHorizontalX -= cameraWidth;
        }
        
        // Smooth horizontal movement with overshoot
        float distance = Mathf.Abs(targetHorizontalX - targetPosition.x);
        if (distance > 0.01f)
        {
            // Use SmoothDamp for smooth acceleration and deceleration with overshoot
            float smoothTime = 1f / horizontalTransitionSpeed;
            float maxSpeed = Mathf.Infinity;
            
            targetPosition.x = Mathf.SmoothDamp(
                targetPosition.x, 
                targetHorizontalX, 
                ref currentVelocity.x, 
                smoothTime, 
                maxSpeed, 
                Time.deltaTime
            );
            
            // Add subtle overshoot effect
            if (distance < cameraWidth * 0.5f && Mathf.Abs(currentVelocity.x) > 0.1f)
            {
                targetPosition.x += currentVelocity.x * overshootAmount * Time.deltaTime;
            }
        }
        else
        {
            targetPosition.x = targetHorizontalX;
            currentVelocity.x = 0f;
        }
    }
    
    void HandleVerticalMovement()
    {
        float playerY = player.position.y;
        float cameraY = targetPosition.y;
        
        // Calculate vertical zones
        float quarterHeight = cameraHeight / 4f;
        float lowerBound = cameraY - quarterHeight;  // 25% mark (bottom of comfort zone)
        float upperBound = cameraY;                  // 50% mark (middle of camera)
        
        // If player goes above middle (50%), follow upward
        if (playerY > upperBound)
        {
            float desiredY = playerY;
            targetPosition.y = Mathf.Lerp(targetPosition.y, desiredY, verticalSmoothSpeed * Time.deltaTime);
        }
        // If player goes below 25% mark, follow downward
        else if (playerY < lowerBound)
        {
            float desiredY = playerY + quarterHeight; // Keep player at 25% mark
            targetPosition.y = Mathf.Lerp(targetPosition.y, desiredY, verticalSmoothSpeed * Time.deltaTime);
        }
        // Between 25% and 50% = comfort zone, camera doesn't move
    }
    
    void ApplyBounds()
    {
        // Clamp camera position to never go outside level bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
    }
    
    void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>();
        
        // Calculate camera dimensions
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        
        // Camera's position is the CENTER of the bottom-left valid camera position
        Vector3 cameraCenterPos = transform.position;
        
        // Calculate level bounds (outer rectangle) around all camera positions
        float totalWidth = width * boundsWidth;
        float totalHeight = height * boundsHeight;
        
        // The bounds center needs to account for camera being a center point
        Vector3 boundsCenter = new Vector3(
            cameraCenterPos.x + (totalWidth - width) / 2f,
            cameraCenterPos.y + (totalHeight - height) / 2f,
            0
        );
        
        // Draw camera grid sections
        Gizmos.color = Color.yellow;
        for (int x = 0; x < boundsWidth; x++)
        {
            for (int y = 0; y < boundsHeight; y++)
            {
                float posX = cameraCenterPos.x + x * width;
                float posY = cameraCenterPos.y + y * height;
                
                Vector3 sectionCenter = new Vector3(posX, posY, 0);
                Gizmos.DrawWireCube(sectionCenter, new Vector3(width, height, 0));
                
                // Draw vertical comfort zone (25% to 50% marks)
                Gizmos.color = Color.green;
                float quarterHeight = height / 4f;
                float comfortZoneBottom = posY - quarterHeight;
                float comfortZoneTop = posY;
                Gizmos.DrawLine(new Vector3(posX - width/2f, comfortZoneBottom, 0), 
                               new Vector3(posX + width/2f, comfortZoneBottom, 0));
                Gizmos.DrawLine(new Vector3(posX - width/2f, comfortZoneTop, 0), 
                               new Vector3(posX + width/2f, comfortZoneTop, 0));
                
                Gizmos.color = Color.yellow;
            }
        }
        
        // Draw level bounds (outer rectangle) LAST so it's visible
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boundsCenter, new Vector3(totalWidth, totalHeight, 0));
    }
}
