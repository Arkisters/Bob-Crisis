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
        transform.position = startPosition;
        transform.eulerAngles = new Vector3(0, 0, startRotation);
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
        if (constantRotationSpeed != 0f)
        {
            transform.Rotate(Vector3.forward, constantRotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    void UpdateWithNewSystem()
    {
        if (waypointData == null || waypointData.Count == 0) return;
        
        // Store position before moving
        Vector3 positionBeforeMove = transform.position;
        
        // Get target waypoint (the one we're moving TO)
        int targetIndex = (currentWaypointIndex + 1) % waypointData.Count;
        WaypointData targetWaypoint = waypointData[targetIndex];
        Vector3 targetPosition = targetWaypoint.position;
        // Use the speed defined on the target waypoint (speed TO that waypoint)
        float targetSpeed = targetWaypoint.speed > 0 ? targetWaypoint.speed : moveSpeed;
        
        // Move platform toward target waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, targetSpeed * Time.fixedDeltaTime);

        // Apply calculated rotation (independent of constant rotation)
        if (currentRotationSpeed != 0f)
        {
            transform.Rotate(Vector3.forward, currentRotationSpeed * Time.fixedDeltaTime);
        }

        // Calculate velocity AFTER moving
        platformVelocity = (transform.position - positionBeforeMove) / Time.fixedDeltaTime;
        
        // Check if reached target waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // Only snap rotation if there was a rotation change to this waypoint
            // Check if the previous and current waypoint have different rotations
            WaypointData previousWaypoint = waypointData[currentWaypointIndex];
            float rotationDiff = Mathf.DeltaAngle(previousWaypoint.rotation, targetWaypoint.rotation);
            if (Mathf.Abs(rotationDiff) > 0.01f)
            {
                // There was a rotation change, snap to exact target
                transform.eulerAngles = new Vector3(0, 0, targetWaypoint.rotation);
            }
            // Otherwise, don't snap - let constant rotation continue freely
            
            // Move to next waypoint
            currentWaypointIndex = targetIndex;
            
            // Calculate rotation speed for next segment
            CalculateRotationSpeed();
        }
    }
    
    void CalculateRotationSpeed()
    {
        if (waypointData == null || waypointData.Count == 0) return;
        
        // Get current waypoint (where we are) and next waypoint (where we're going TO)
        int nextIndex = (currentWaypointIndex + 1) % waypointData.Count;
        WaypointData currentWaypoint = waypointData[currentWaypointIndex];
        WaypointData nextWaypoint = waypointData[nextIndex];
        
        // Check if rotation will change at the next waypoint
        float rotationDifference = Mathf.DeltaAngle(currentWaypoint.rotation, nextWaypoint.rotation);
        
        // Only rotate if the next waypoint has a different rotation than current
        if (Mathf.Abs(rotationDifference) < 0.01f)
        {
            currentRotationSpeed = 0f;
            return;
        }
        
        // Calculate distance from current waypoint to next waypoint
        float distance = Vector3.Distance(currentWaypoint.position, nextWaypoint.position);
        
        // Use the speed defined on the next waypoint (speed TO that waypoint)
        float targetSpeed = nextWaypoint.speed > 0 ? nextWaypoint.speed : moveSpeed;
        
        // Calculate time to reach next waypoint: time = distance / speed
        if (distance > 0.01f && targetSpeed > 0)
        {
            float timeToReach = distance / targetSpeed;
            // Calculate rotation speed: degrees per second = total rotation / time
            currentRotationSpeed = rotationDifference / timeToReach;
        }
        else
        {
            currentRotationSpeed = 0f;
        }
    }
    
    void UpdateWithOldSystem()
    {
        // Store position before moving
        Vector3 positionBeforeMove = transform.position;
        
        // Move platform toward current waypoint
        Vector3 targetPosition = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

        // Calculate velocity AFTER moving
        platformVelocity = (transform.position - positionBeforeMove) / Time.fixedDeltaTime;
        
        // Check if reached current waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0;
            }
        }
    }
    
    public Vector3 GetVelocity()
    {
        return platformVelocity;
    }
}
