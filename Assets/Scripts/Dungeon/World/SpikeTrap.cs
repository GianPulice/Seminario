using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float spikeDistance = 1.5f;
    [SerializeField] private float spikeSpeed = 5f;
    [SerializeField] private float resetDelay = 1f;
    [SerializeField] private float cooldownTime = 2f;

    [Header("Timing")]
    [SerializeField] private float damageDelay = 0.5f; // ventana para esquivar

    [Header("References")]
    [SerializeField] private Transform spikeMesh;  // el objeto que se mueve

    private Vector3 originalLocalPos;
    private bool isActive = false;
    private float cooldownTimer = 0f;

    // control de daño diferido
    private PlayerDungeonModel currentTarget;
    private Coroutine pendingDamageRoutine;

    private void Start()
    {
        if (!spikeMesh)
        {
            Debug.LogError("SpikeTrap: Asigna 'spikeMesh' en el inspector.");
            enabled = false;
            return;
        }
        originalLocalPos = spikeMesh.localPosition;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive || cooldownTimer > 0f) return;

        if (other.TryGetComponent<PlayerDungeonModel>(out var player))
        {
            isActive = true;
            currentTarget = player;

            // Dirección en la que apuntan los pinchos (local Z+ del spikeMesh)
            Vector3 targetLocalPos = originalLocalPos + (spikeMesh.localRotation * Vector3.up) * spikeDistance;

            StartCoroutine(SpikeMovement(targetLocalPos));
            pendingDamageRoutine = StartCoroutine(DelayedDamage());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentTarget != null && other.GetComponent<PlayerDungeonModel>() == currentTarget)
        {
            if (pendingDamageRoutine != null)
            {
                StopCoroutine(pendingDamageRoutine);
                pendingDamageRoutine = null;
            }
            currentTarget = null;
        }
    }

    private IEnumerator DelayedDamage()
    {
        yield return new WaitForSeconds(damageDelay);

        if (currentTarget != null)
        {
            currentTarget.TakeDamage(damage);
        }

        pendingDamageRoutine = null;
    }

    private IEnumerator SpikeMovement(Vector3 targetLocalPos)
    {
        // ida
        Vector3 start = spikeMesh.localPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * spikeSpeed;
            spikeMesh.localPosition = Vector3.Lerp(start, targetLocalPos, t);
            yield return null;
        }

        // espera
        yield return new WaitForSeconds(resetDelay);

        // vuelta
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * spikeSpeed;
            spikeMesh.localPosition = Vector3.Lerp(targetLocalPos, originalLocalPos, t);
            yield return null;
        }

        // limpieza
        cooldownTimer = cooldownTime;
        isActive = false;
        currentTarget = null;

        if (pendingDamageRoutine != null)
        {
            StopCoroutine(pendingDamageRoutine);
            pendingDamageRoutine = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spikeMesh)
        {
            Gizmos.color = Color.red;
            Vector3 p = spikeMesh.position;
            Vector3 dir = spikeMesh.forward;
            Gizmos.DrawLine(p, p + dir * 1.0f);
            Gizmos.DrawSphere(p + dir * 1.0f, 0.05f);
        }
    }
#endif
}
