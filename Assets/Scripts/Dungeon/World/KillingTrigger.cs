using UnityEngine;

public class KillingTrigger : MonoBehaviour
{
    [Range(1,100)]public int damageOnDeath = 50;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.TryGetComponent<SafetyPoint>(out SafetyPoint safetyPoint))
            {
                safetyPoint.TeleportInitiate(damageOnDeath);
            }
            else
            {
                Debug.LogWarning("SafetyPoint component not found on the player.");
            }
        }
    }
}
