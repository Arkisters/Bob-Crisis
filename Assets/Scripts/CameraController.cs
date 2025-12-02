using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    
    [Header("Level Bounds (in camera units)")]
    public int boundsWidth = 2;
    public int boundsHeight = 2;
    
    [Header("Vertical Follow Settings")]
    [Tooltip("Base smooth speed for vertical camera movement")]
    public float verticalSmoothSpeed = 3f;
    [Tooltip("How much speed increases per unit distance below sweet spot (smooth curve)")]
    public float speedIncreasePerUnit = 0.3f;
    [Tooltip("Speed when returning to sweet spot after falling stops")]
    public float returnToSweetSpotSpeed = 2f;
    
    [Header("Dynamic Sweet Spot (Fall Response)")]
    [Tooltip("Minimum downward velocity before sweet spot starts moving up")]
    public float minVelocityForSweetSpotShift = 5f;
    [Tooltip("Maximum downward velocity for sweet spot calculation (terminal velocity)")]
    public float maxDownwardVelocity = 9.6f;
    [Tooltip("How quickly sweet spot returns to default position when velocity decreases")]
    public float sweetSpotReturnSpeed = 5f;
    
    [Header("Horizontal Follow Settings")]
    [Tooltip("Use smooth follow (false) or sectioned camera that snaps to grid positions (true)")]
    public bool useSectionedCamera = false;
    [Tooltip("Smooth speed for horizontal camera movement (only used if not sectioned)")]
    public float horizontalSmoothSpeed = 2f;
    [Tooltip("Speed for transitioning between sections (only used if sectioned)")]
    public float sectionTransitionSpeed = 5f;
    
    private Camera cam;
    private float cameraWidth;
    private float cameraHeight;
    private Vector3 targetPosition;
    private float minX, maxX, minY, maxY;
    private float currentSweetSpotOffset = 0f;  // Tracks current dynamic sweet spot offset
    private int currentSectionIndex = 0;  // Tracks which horizontal section we're in
    
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
        
        // Initialize section index based on starting position
        currentSectionIndex = Mathf.RoundToInt((targetPosition.x - minX) / cameraWidth);
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
        if (useSectionedCamera)
        {
            // Sectioned camera: Snap to discrete camera sections
            // Calculate target section position
            float targetSectionX = minX + (currentSectionIndex * cameraWidth);
            
            // Check if player has exited the current section bounds
            float sectionLeft = targetSectionX - cameraWidth / 2f;
            float sectionRight = targetSectionX + cameraWidth / 2f;
            
            // If player exits left edge, move to previous section
            if (player.position.x < sectionLeft && currentSectionIndex > 0)
            {
                currentSectionIndex--;
                targetSectionX = minX + (currentSectionIndex * cameraWidth);
            }
            // If player exits right edge, move to next section
            else if (player.position.x > sectionRight && currentSectionIndex < boundsWidth - 1)
            {
                currentSectionIndex++;
                targetSectionX = minX + (currentSectionIndex * cameraWidth);
            }
            
            // Smoothly transition to target section
            targetPosition.x = Mathf.Lerp(targetPosition.x, targetSectionX, sectionTransitionSpeed * Time.deltaTime);
        }
        else
        {
            // Smooth follow: Continuously follow player horizontally
            targetPosition.x = Mathf.Lerp(targetPosition.x, player.position.x, horizontalSmoothSpeed * Time.deltaTime);
        }
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
        
        // Calculate dynamic sweet spot offset based on downward velocity
        // At minVelocity (5f): sweet spot barely moves
        // At maxVelocity (9.6f): sweet spot moves up by 6 sprites (from sprite 3 to sprite 9, mirroring original distance from bottom)
        float targetSweetSpotOffset = 0f;
        if (playerVelocityY < -minVelocityForSweetSpotShift)
        {
            // Normalize velocity between min and max
            float velocityRange = maxDownwardVelocity - minVelocityForSweetSpotShift;
            float normalizedVelocity = Mathf.Clamp01((Mathf.Abs(playerVelocityY) - minVelocityForSweetSpotShift) / velocityRange);
            
            // At max velocity, sweet spot should be 6 sprites higher (sprite 3 → sprite 9)
            // This mirrors the original 3 sprite distance from bottom (now 3 sprites from top)
            targetSweetSpotOffset = normalizedVelocity * spriteHeight * 6f;
        }
        
        // Smoothly interpolate current offset toward target
        currentSweetSpotOffset = Mathf.Lerp(currentSweetSpotOffset, targetSweetSpotOffset, sweetSpotReturnSpeed * Time.deltaTime);
        
        // Apply dynamic offset to sweet spot position (move it UP when falling fast)
        float dynamicSweetSpotHeight = 3f + (currentSweetSpotOffset / spriteHeight);
        
        // Calculate vertical zones based on dynamic sweet spot
        // Comfort zone: 2 sprites below and 2 sprites above dynamic sweet spot
        float sweetSpotY = cameraY - cameraHeight / 2f + spriteHeight * dynamicSweetSpotHeight;
        float lowerBound = sweetSpotY - spriteHeight * 1f;  // 1 sprite below sweet spot
        float upperBound = sweetSpotY + spriteHeight * 2f;  // 2 sprites above sweet spot
        
        // Calculate where camera should be to place player at dynamic sweet spot
        float idealCameraY = playerY - spriteHeight * dynamicSweetSpotHeight + cameraHeight / 2f;
        
        // Calculate distance from sweet spot for speed scaling
        float distanceFromSweetSpot = Mathf.Abs(sweetSpotY - playerY);
        float distanceBelowSweetSpot = Mathf.Max(0, sweetSpotY - playerY);
        
        // Speed increases based on distance below sweet spot
        // The further below, the faster the camera moves
        float speedMultiplier = 1f + (distanceBelowSweetSpot * speedIncreasePerUnit);
        
        // Movement logic
        if (playerY > upperBound)
        {
            // Player above comfort zone - follow upward
            float effectiveSpeed = verticalSmoothSpeed * speedMultiplier;
            targetPosition.y = Mathf.Lerp(targetPosition.y, idealCameraY, effectiveSpeed * Time.deltaTime);
        }
        else if (playerY < lowerBound)
        {
            // Player below comfort zone - catch up faster the further they are
            float effectiveSpeed = verticalSmoothSpeed * speedMultiplier;
            targetPosition.y = Mathf.Lerp(targetPosition.y, idealCameraY, effectiveSpeed * Time.deltaTime);
        }
        else if (Mathf.Abs(cameraY - idealCameraY) > spriteHeight * 0.1f)
        {
            // Player IN comfort zone but camera not quite at ideal position
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
        float spriteHeight = height / 9f;
        
        // Camera's position is the CENTER of the camera
        Vector3 cameraCenterPos = transform.position;
        
        // Calculate level bounds (outer rectangle) - stays fixed at original position
        Vector3 startPos = Application.isPlaying ? new Vector3(minX, minY, 0) : cameraCenterPos;
        float totalWidth = width * boundsWidth;
        float totalHeight = height * boundsHeight;
        
        Vector3 boundsCenter = new Vector3(
            startPos.x + (totalWidth - width) / 2f,
            startPos.y + (totalHeight - height) / 2f,
            0
        );
        
        if (Application.isPlaying)
        {
            // PLAY MODE: Show only current camera position, comfort zone, and fixed outer bounds
            
            // Draw current camera section (yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(cameraCenterPos, new Vector3(width, height, 0));
            
            // Draw dynamic comfort zone (green)
            float dynamicSweetSpotHeight = 3f + (currentSweetSpotOffset / spriteHeight);
            float sectionBottom = cameraCenterPos.y - height / 2f;
            float sweetSpotY = sectionBottom + spriteHeight * dynamicSweetSpotHeight;
            float lowerBound = sweetSpotY - spriteHeight * 1f;
            float upperBound = sweetSpotY + spriteHeight * 2f;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(cameraCenterPos.x - width/2f, lowerBound, 0), 
                           new Vector3(cameraCenterPos.x + width/2f, lowerBound, 0));
            Gizmos.DrawLine(new Vector3(cameraCenterPos.x - width/2f, upperBound, 0), 
                           new Vector3(cameraCenterPos.x + width/2f, upperBound, 0));
            
            // Draw fixed level bounds (red)
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boundsCenter, new Vector3(totalWidth, totalHeight, 0));
        }
        else
        {
            // EDIT MODE: Show only current camera position (yellow) and outer bounds (red)
            
            // Draw current camera section (yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(cameraCenterPos, new Vector3(width, height, 0));
            
            // If sectioned camera is enabled, draw all vertical section dividers
            if (useSectionedCamera)
            {
                Gizmos.color = Color.yellow;
                // Draw lines at the edges between sections, from bottom to top of level bounds
                for (int i = 0; i <= boundsWidth; i++)
                {
                    // Calculate position at the left edge of each section (which is the right edge of the previous section)
                    float dividerX = startPos.x + (i * width) - width / 2f;
                    Vector3 lineTop = new Vector3(dividerX, startPos.y + totalHeight - height / 2f, 0);
                    Vector3 lineBottom = new Vector3(dividerX, startPos.y - height / 2f, 0);
                    Gizmos.DrawLine(lineBottom, lineTop);
                }
            }
            
            // Draw level bounds (red)
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boundsCenter, new Vector3(totalWidth, totalHeight, 0));
        }
    }
}
