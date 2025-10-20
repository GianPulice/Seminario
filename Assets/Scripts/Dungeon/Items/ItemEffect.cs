using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffect : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.5f;

    private Vector3 startPosition;
    private float bobTime = 0f;

    private void Awake()
    {
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime);

        bobTime += Time.deltaTime * bobSpeed;
        float offsetY = Mathf.Sin(bobTime) * bobHeight;

        float newY = startPosition.y + offsetY;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

    }
}
