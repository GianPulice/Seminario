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
    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Points A and B must be assigned to LineMoveObject.");
            enabled = false;
            return;
        }

        startPosition = pointA.position;
        endPosition = pointB.position;
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;

        // Calcula un valor oscilante entre 0 y 1 usando el seno del tiempo
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        // Interpola la posición entre A y B según t
        transform.position = Vector3.Lerp(startPosition, endPosition, t);
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
