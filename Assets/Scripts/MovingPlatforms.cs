using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaypointData
{
    public Vector3 position;
    public float speed = 2f;
    
    public WaypointData(Vector3 pos, float spd = 2f)
    {
        position = pos;
        speed = spd;
    }
}

public class MovingPlatforms : MonoBehaviour
{
    public float moveSpeed = 2f; // Default speed (kept for backwards compatibility)
    public List<Vector3> waypoints = new List<Vector3>(); // Old waypoint system
    public List<WaypointData> waypointData = new List<WaypointData>(); // New waypoint system
    
    private int currentWaypointIndex;
    private Vector3 platformVelocity;
    private Vector3 startPosition;
    private float currentSpeed;
    
    // Use new waypoint system if available, otherwise fall back to old
    private bool UseNewSystem => waypointData != null && waypointData.Count > 0;

    void Awake()
    {
        // Cache the start position immediately (Awake runs even if the object starts disabled).
        if (UseNewSystem)
            startPosition = waypointData[0].position;
        else if (waypoints != null && waypoints.Count > 0)
            startPosition = waypoints[0];
        else
            startPosition = transform.position;
    }

    void OnEnable()
    {
        // Reset platform state every time the object is enabled.
        transform.position = startPosition;
        currentWaypointIndex = 0;
        platformVelocity = Vector3.zero;
        currentSpeed = UseNewSystem ? waypointData[0].speed : moveSpeed;
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
    }
    
    void UpdateWithNewSystem()
    {
        if (waypointData == null || waypointData.Count == 0) return;
        
        // Store position before moving
        Vector3 positionBeforeMove = transform.position;
        
        // Get current waypoint data
        WaypointData currentWaypoint = waypointData[currentWaypointIndex];
        Vector3 targetPosition = currentWaypoint.position;
        float targetSpeed = currentWaypoint.speed > 0 ? currentWaypoint.speed : moveSpeed; // Use moveSpeed as default if speed is 0
        
        // Move platform toward current waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, targetSpeed * Time.fixedDeltaTime);

        // Calculate velocity AFTER moving
        platformVelocity = (transform.position - positionBeforeMove) / Time.fixedDeltaTime;
        
        // Check if reached current waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypointData.Count)
            {
                currentWaypointIndex = 0;
            }
            // Update speed for next segment
            currentSpeed = waypointData[currentWaypointIndex].speed;
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
