using UnityEngine;
using System.Collections.Generic;

public class MovingPlatforms : MonoBehaviour
{
    public float moveSpeed = 2f;
    public List<Vector3> waypoints = new List<Vector3>();
    
    private int currentWaypointIndex;
    private Vector3 platformVelocity;
    private Vector3 startPosition;

    void Start()
    {
        if (waypoints.Count > 0)
        {
            // Store the first waypoint as start position
            startPosition = waypoints[0];
            transform.position = startPosition;
            currentWaypointIndex = 0;
        }
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;
        
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
