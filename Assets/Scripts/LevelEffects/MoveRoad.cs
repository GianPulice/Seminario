using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRoad : MonoBehaviour
{
    [Header("Movement Configuration")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float waitTime = 0f;
    
    [Header("Rotation Configuration")]
    [SerializeField] private float[] rotationAngles;
    [SerializeField] private float rotationSpeed = 90f;
    
    [Header("Loop Configuration")]
    [SerializeField] private bool loopMovement = true;
    
    private int currentWaypointIndex = 0;
    private bool isMoving = true;
    private bool isWaiting = false;
    private bool isRotating = false;
    private Quaternion targetRotation;
    private Quaternion originalRotation;
    
    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned in " + gameObject.name);
            enabled = false;
            return;
        }
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                Debug.LogError("Waypoint " + i + " is not assigned in " + gameObject.name);
                enabled = false;
                return;
            }
        }
        
        transform.position = waypoints[0].position;
        originalRotation = transform.rotation;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (!isMoving || waypoints == null || waypoints.Length == 0)
            return;
            
        if (isRotating)
        {
            RotateToTarget();
        }
        else
        {
            MoveToWaypoint();
        }
    }
    
    private void MoveToWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        float distanceToTarget = Vector3.Distance(transform.position, targetWaypoint.position);
        
        if (distanceToTarget < 0.1f)
        {
            transform.position = targetWaypoint.position;
            
            if (ShouldRotateAtWaypoint())
            {
                StartRotation();
            }
            else if (waitTime > 0 && !isWaiting)
            {
                StartCoroutine(WaitAtWaypoint());
            }
            else
            {
                MoveToNextWaypoint();
            }
        }
    }
    
    private void MoveToNextWaypoint()
    {
        currentWaypointIndex++;
        
        if (currentWaypointIndex >= waypoints.Length)
        {
            if (loopMovement)
            {
                currentWaypointIndex = 0;
                transform.position = waypoints[0].position;
            }
            else
            {
                isMoving = false;
            }
        }
    }
    
    private bool ShouldRotateAtWaypoint()
    {
        if (currentWaypointIndex == 0)
        {
            return true;
        }
        
        return rotationAngles != null && 
               currentWaypointIndex < rotationAngles.Length && 
               rotationAngles[currentWaypointIndex] != 0f;
    }
    
    private void StartRotation()
    {
        if (currentWaypointIndex == 0)
        {
            targetRotation = originalRotation;
        }
        else
        {
            float rotationAngle = rotationAngles[currentWaypointIndex];
            targetRotation = transform.rotation * Quaternion.Euler(0, rotationAngle, 0);
        }
        isRotating = true;
    }
    
    private void RotateToTarget()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            transform.rotation = targetRotation;
            isRotating = false;
            
            if (waitTime > 0 && !isWaiting)
            {
                StartCoroutine(WaitAtWaypoint());
            }
            else
            {
                MoveToNextWaypoint();
            }
        }
    }
    
    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        MoveToNextWaypoint();
    }
    
    public void RestartMovement()
    {
        currentWaypointIndex = 0;
        isMoving = true;
        isWaiting = false;
        isRotating = false;
        transform.position = waypoints[0].position;
        targetRotation = originalRotation;
    }
    
    public void SetMovementActive(bool active)
    {
        isMoving = active;
    }
    
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}
