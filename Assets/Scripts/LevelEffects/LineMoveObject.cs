using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMoveObject : MonoBehaviour
{
    [Header("Start Point (A)")]
    public Transform pointA;
    [Header("End Point (B)")]
    public Transform pointB;
    [Header("Movement Speed (Rate of change)")]
    [Tooltip("La velocidad determina la tasa de suavizado. Un valor más alto significa un movimiento más rápido y menos 'deslizante'.")]
    public float speed = 1f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 currentTarget;

    private float totalDistance;
    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Points A and B must be assigned to LineMoveObject.");
            enabled = false;
            return;
        }

        // Inicializar posiciones
        startPosition = pointA.position;
        endPosition = pointB.position;

        // Colocar el objeto en el punto inicial y definir el primer objetivo
        transform.position = startPosition;
        currentTarget = endPosition;

        totalDistance = Vector3.Distance(startPosition, endPosition);
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;
        transform.position = Vector3.Lerp(transform.position,currentTarget,speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
        {
            if (currentTarget == endPosition)
            {
                currentTarget = startPosition;
            }
            else
            {
                currentTarget = endPosition;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.1f);
        }
        if (pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.1f);
        }
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
