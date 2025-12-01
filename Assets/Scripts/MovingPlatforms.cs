using UnityEngine;
using System.Collections.Generic;

public class MovingPlatforms : MonoBehaviour
{
    public float moveSpeed = 2f;
    public List<Vector3> waypoints = new List<Vector3>();
    
    private int currentWaypointIndex;
    private Vector3 platformVelocity;
    private Vector3 startPosition;

    void Awake()
    {
        // Cache the start position immediately (Awake runs even if the object starts disabled).
        if (waypoints != null && waypoints.Count > 0)
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
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        
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
