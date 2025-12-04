using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaypointData
{
    public Vector3 position;
    public float speed = 2f;
    public float rotation = 0f; // Target rotation at this waypoint (in degrees)
    
    public WaypointData(Vector3 pos, float spd = 2f, float rot = 0f)
    {
        position = pos;
        speed = spd;
        rotation = rot;
    }
}

public class MovingPlatforms : MonoBehaviour
{
    public float moveSpeed = 2f; // Default speed (kept for backwards compatibility)
    public float constantRotationSpeed = 0f; // Constant rotation (positive = clockwise, negative = counter-clockwise)
    public List<Vector3> waypoints = new List<Vector3>(); // Old waypoint system
    public List<WaypointData> waypointData = new List<WaypointData>(); // New waypoint system
    
    private Rigidbody2D rb;
    private int currentWaypointIndex;
    private Vector3 platformVelocity;
    private Vector3 startPosition;
    private float startRotation;
    private float currentSpeed;
    private float currentRotationSpeed; // Calculated rotation speed to reach target rotation
    
    // Use new waypoint system if available, otherwise fall back to old
    private bool UseNewSystem => waypointData != null && waypointData.Count > 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("MovingPlatforms requires a Rigidbody2D component!");
        }
        
        // Cache the start position immediately (Awake runs even if the object starts disabled).
        if (UseNewSystem)
        {
            startPosition = waypointData[0].position;
            startRotation = waypointData[0].rotation;
        }
        else if (waypoints != null && waypoints.Count > 0)
        {
            startPosition = waypoints[0];
            startRotation = transform.eulerAngles.z;
        }
        else
        {
            startPosition = transform.position;
            startRotation = transform.eulerAngles.z;
        }
    }

    void OnEnable()
    {
        // Reset platform state every time the object is enabled.
        if (rb != null)
        {
            rb.MovePosition(startPosition);
            rb.MoveRotation(startRotation);
        }
        currentWaypointIndex = 0;
        platformVelocity = Vector3.zero;
        currentSpeed = UseNewSystem ? waypointData[0].speed : moveSpeed;
        
        if (UseNewSystem && waypointData.Count > 1)
        {
            CalculateRotationSpeed();
        }
    }

    void FixedUpdate()
    {
        if (UseNewSystem)
        {
            UpdateWithNewSystem();
        }
        else if (waypoints != null && waypoints.Count > 0)
        {
            UpdateWithOldSystem();
        }
        
        // Apply constant rotation if set
        if (constantRotationSpeed != 0f && rb != null)
        {
            rb.MoveRotation(rb.rotation + constantRotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    void UpdateWithNewSystem()
    {
        if (waypointData == null || waypointData.Count == 0 || rb == null) return;
        
        // Store position before moving
        Vector3 positionBeforeMove = rb.position;
        
        // Get target waypoint (the one we're moving TO)
        int targetIndex = (currentWaypointIndex + 1) % waypointData.Count;
        WaypointData targetWaypoint = waypointData[targetIndex];
        float targetSpeed = targetWaypoint.speed > 0 ? targetWaypoint.speed : moveSpeed;
        
        // Move platform toward target waypoint using Rigidbody2D
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetWaypoint.position, targetSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Apply calculated rotation (independent of constant rotation)
        if (currentRotationSpeed != 0f)
        {
            rb.MoveRotation(rb.rotation + currentRotationSpeed * Time.fixedDeltaTime);
        }

        // Calculate velocity AFTER moving
        platformVelocity = (newPosition - positionBeforeMove) / Time.fixedDeltaTime;
        
        // Check if reached target waypoint
        if (Vector3.Distance(rb.position, targetWaypoint.position) < 0.01f)
        {
            // Only snap rotation if there was a rotation change to this waypoint
            WaypointData previousWaypoint = waypointData[currentWaypointIndex];
            float rotationDiff = Mathf.DeltaAngle(previousWaypoint.rotation, targetWaypoint.rotation);
            if (Mathf.Abs(rotationDiff) > 0.01f)
            {
                rb.MoveRotation(targetWaypoint.rotation);
            }
            
            // Move to next waypoint
            currentWaypointIndex = targetIndex;
            CalculateRotationSpeed();
        }
    }
    
    void CalculateRotationSpeed()
    {
        if (waypointData == null || waypointData.Count == 0) return;
        
        int nextIndex = (currentWaypointIndex + 1) % waypointData.Count;
        WaypointData currentWaypoint = waypointData[currentWaypointIndex];
        WaypointData nextWaypoint = waypointData[nextIndex];
        
        float rotationDifference = Mathf.DeltaAngle(currentWaypoint.rotation, nextWaypoint.rotation);
        
        if (Mathf.Abs(rotationDifference) < 0.01f)
        {
            currentRotationSpeed = 0f;
            return;
        }
        
        float distance = Vector3.Distance(currentWaypoint.position, nextWaypoint.position);
        float targetSpeed = nextWaypoint.speed > 0 ? nextWaypoint.speed : moveSpeed;
        
        if (distance > 0.01f && targetSpeed > 0)
        {
            currentRotationSpeed = rotationDifference / (distance / targetSpeed);
        }
        else
        {
            currentRotationSpeed = 0f;
        }
    }
    
    void UpdateWithOldSystem()
    {
        if (rb == null) return;
        
        Vector3 positionBeforeMove = rb.position;
        Vector3 targetPosition = waypoints[currentWaypointIndex];
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        platformVelocity = (newPosition - positionBeforeMove) / Time.fixedDeltaTime;
        
        if (Vector3.Distance(rb.position, targetPosition) < 0.01f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
    }
    
    public Vector3 GetVelocity()
    {
        return platformVelocity;
    }
}
