using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    
    [Header("Level Bounds (in camera units)")]
    public int boundsWidth = 2;
    public int boundsHeight = 2;
    
    [Header("Vertical Follow Settings")]
    [Tooltip("Base smooth speed for vertical camera movement (non-fast-fall situations)")]
    public float verticalSmoothSpeed = 3f;
    [Tooltip("Base speed when following fast falls (independent of verticalSmoothSpeed)")]
    public float fastFallBaseSpeed = 2f;
    [Tooltip("How much speed increases per unit distance below sweet spot center (smooth curve)")]
    public float speedIncreasePerUnit = 0.2f;
    [Tooltip("How much further down camera looks when falling (in sprite heights)")]
    public float fallLookaheadDistance = 5f;
    [Tooltip("Speed when returning to sweet spot after falling stops")]
    public float returnToSweetSpotSpeed = 3f;
    [Tooltip("Minimum fall speed before lookahead kicks in")]
    public float minFallSpeedForLookahead = 9.2f;
    
    [Header("Horizontal Follow Settings")]
    [Tooltip("Smooth speed for horizontal camera movement")]
    public float horizontalSmoothSpeed = 2f;
    
    private Camera cam;
    private float cameraWidth;
    private float cameraHeight;
    private Vector3 targetPosition;
    private float minX, maxX, minY, maxY;
    
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
        // Smoothly follow player horizontally
        targetPosition.x = Mathf.Lerp(targetPosition.x, player.position.x, horizontalSmoothSpeed * Time.deltaTime);
    }
    
    void HandleVerticalMovement()
    {
        float playerY = player.position.y;
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        float playerVelocityY = playerRb != null ? playerRb.linearVelocity.y : 0f;
        float cameraY = targetPosition.y;
        
        // Sweet spot is 3rd sprite from bottom (out of 9 sprites tall)
        // Camera height = 9 sprites, so each sprite = cameraHeight / 9
        float spriteHeight = cameraHeight / 9f;
        
        // Calculate vertical zones based on 9-sprite grid relative to CURRENT camera
        // Comfort zone: sprites 2-5
        float lowerBound = cameraY - cameraHeight / 2f + spriteHeight * 2f;  // Bottom of 2nd sprite
        float upperBound = cameraY - cameraHeight / 2f + spriteHeight * 5f;  // Top of 5th sprite
        float sweetSpotCenter = cameraY - cameraHeight / 2f + spriteHeight * 3f;  // Center of 3rd sprite
        
        // Calculate where camera should be to place player at sweet spot (3rd sprite from bottom)
        float idealCameraY = playerY - spriteHeight * 3f + cameraHeight / 2f;
        
        // Determine if player is falling fast
        bool isFallingFast = playerVelocityY < -minFallSpeedForLookahead;
        
        // When falling fast, target position that places player ABOVE sweet spot (e.g., at 4th sprite)
        float targetCameraY = idealCameraY;
        if (isFallingFast)
        {
            // Place player at 4th sprite instead of 3rd (1 sprite higher)
            targetCameraY = playerY - spriteHeight * 4f + cameraHeight / 2f;
            // Then apply lookahead distance on top
            targetCameraY -= spriteHeight * fallLookaheadDistance;
        }
        
        // Calculate speed increase based on distance below sweet spot center
        // Continuous smooth curve: speed = baseSpeed + (distance * speedIncreasePerUnit)
        float distanceBelowCenter = Mathf.Max(0, sweetSpotCenter - playerY);
        
        // Priority system:
        // 1. Player above comfort zone → follow upward
        // 2. Falling fast → apply lookahead (regardless of position in comfort zone)
        // 3. Player below comfort zone (not falling fast) → catch up
        // 4. Camera below ideal (player stopped) → return to sweet spot
        
        if (playerY > upperBound)
        {
            // Player above comfort zone - follow upward at base speed
            targetPosition.y = Mathf.Lerp(targetPosition.y, idealCameraY, verticalSmoothSpeed * Time.deltaTime);
        }
        else if (isFallingFast)
        {
            // Player falling fast - use independent speed control
            // Speed = baseSpeed + gradual increase based on distance below center
            float effectiveSpeed = fastFallBaseSpeed + (distanceBelowCenter * speedIncreasePerUnit);
            targetPosition.y = Mathf.Lerp(targetPosition.y, targetCameraY, effectiveSpeed * Time.deltaTime);
        }
        else if (playerY < lowerBound)
        {
            // Player below comfort zone (and NOT falling fast) - catch up with smooth curve
            float effectiveSpeed = verticalSmoothSpeed + (distanceBelowCenter * speedIncreasePerUnit);
            targetPosition.y = Mathf.Lerp(targetPosition.y, idealCameraY, effectiveSpeed * Time.deltaTime);
        }
        else if (cameraY < idealCameraY - spriteHeight * 0.2f)
        {
            // Player IN comfort zone, NOT falling fast, but camera is below ideal
            // Return to sweet spot smoothly
            targetPosition.y = Mathf.Lerp(targetPosition.y, idealCameraY, returnToSweetSpotSpeed * Time.deltaTime);
        }
        // Otherwise: player in comfort zone and camera is good, don't move
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
                
                // Draw vertical comfort zone (sprites 2-5 out of 9)
                Gizmos.color = Color.green;
                float spriteHeight = height / 9f;
                float sectionBottom = posY - height / 2f;
                float comfortZoneBottom = sectionBottom + spriteHeight * 2f; // Top of 2nd sprite
                float comfortZoneTop = sectionBottom + spriteHeight * 5f;    // Top of 5th sprite
                
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
