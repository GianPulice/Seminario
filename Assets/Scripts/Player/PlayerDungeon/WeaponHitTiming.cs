using UnityEngine;

/// <summary>
/// Component for managing weapon-specific hit timing.
/// Attach this to weapon GameObjects to override default hit delays.
/// </summary>
public class WeaponHitTiming : MonoBehaviour
{
    [Header("Hit Timing Settings")]
    [Tooltip("Delay before hit detection triggers (in seconds)")]
    [SerializeField] private float hitDelay = 0.3f;
    
    [Tooltip("Random variation added to hit delay (±this value)")]
    [SerializeField] private float delayVariation = 0.0f;
    
    [Tooltip("Minimum hit delay (prevents negative delays)")]
    [SerializeField] private float minDelay = 0.0f;
    
    [Tooltip("Maximum hit delay (prevents excessive delays)")]
    [SerializeField] private float maxDelay = 2.0f;

    [Header("Animation Sync")]
    [Tooltip("Sync hit timing with animation length")]
    [SerializeField] private bool syncWithAnimation = false;
    
    [Tooltip("Hit occurs at this percentage of animation (0.0 = start, 1.0 = end)")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float animationHitPoint = 0.5f;
    
    [Tooltip("Animation length in seconds (used for sync)")]
    [SerializeField] private float animationLength = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Animator weaponAnimator;

    private void Awake()
    {
        weaponAnimator = GetComponent<Animator>();
    }

    /// <summary>
    /// Gets the calculated hit delay for this weapon
    /// </summary>
    public float GetHitDelay()
    {
        float calculatedDelay = hitDelay;

        // Apply animation sync if enabled
        if (syncWithAnimation)
        {
            calculatedDelay = CalculateAnimationSyncDelay();
        }

        // Apply random variation
        if (delayVariation > 0f)
        {
            calculatedDelay += Random.Range(-delayVariation, delayVariation);
        }

        // Clamp to min/max values
        calculatedDelay = Mathf.Clamp(calculatedDelay, minDelay, maxDelay);

        if (showDebugInfo)
        {
            Debug.Log($"[WeaponHitTiming] {gameObject.name}: Hit delay = {calculatedDelay:F3}s");
        }

        return calculatedDelay;
    }

    private float CalculateAnimationSyncDelay()
    {
        // Try to get actual animation length from animator
        float actualAnimationLength = animationLength;
        
        if (weaponAnimator != null)
        {
            // Get current animation state length
            var stateInfo = weaponAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0f)
            {
                actualAnimationLength = stateInfo.length;
            }
        }

        // Calculate hit point based on animation percentage
        return actualAnimationLength * animationHitPoint;
    }

    /// <summary>
    /// Sets the base hit delay
    /// </summary>
    public void SetHitDelay(float newDelay)
    {
        hitDelay = newDelay;
    }

    /// <summary>
    /// Sets the animation hit point (0.0 to 1.0)
    /// </summary>
    public void SetAnimationHitPoint(float hitPoint)
    {
        animationHitPoint = Mathf.Clamp01(hitPoint);
    }

    /// <summary>
    /// Enables or disables animation sync
    /// </summary>
    public void SetSyncWithAnimation(bool sync)
    {
        syncWithAnimation = sync;
    }

    /// <summary>
    /// Gets the current animation length
    /// </summary>
    public float GetAnimationLength()
    {
        if (weaponAnimator != null)
        {
            var stateInfo = weaponAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0f)
            {
                return stateInfo.length;
            }
        }
        return animationLength;
    }

    // Editor helper methods
    [ContextMenu("Test Hit Delay")]
    private void TestHitDelay()
    {
        float delay = GetHitDelay();
        Debug.Log($"[WeaponHitTiming] Test: {gameObject.name} would hit in {delay:F3} seconds");
    }

    [ContextMenu("Sync to Animation")]
    private void SyncToCurrentAnimation()
    {
        if (weaponAnimator != null)
        {
            var stateInfo = weaponAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0f)
            {
                animationLength = stateInfo.length;
                Debug.Log($"[WeaponHitTiming] Synced animation length to {animationLength:F3}s");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // Draw a visual indicator of hit timing
        Gizmos.color = Color.red;
        Vector3 position = transform.position + Vector3.up * 2f;
        Gizmos.DrawWireSphere(position, 0.2f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(position + Vector3.up * 0.5f, $"Hit Delay: {GetHitDelay():F2}s");
        #endif
    }
}
