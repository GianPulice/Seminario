using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private GameObject sword;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Hit Timing")]
    [SerializeField] private bool hasCustomTiming = false;
    [SerializeField] private float customHitDelay = 0.3f;

    [Header("References")]
    [SerializeField] private ShieldHandler shield;

    private Animator animator;
    private float lastAttackTime = -Mathf.Infinity;
    private readonly string[] attackSfxNames = { "Axe_Attack1", "Axe_Attack2"};
    // Animator hashes
    private static readonly int hashAttack = Animator.StringToHash("Attack");
    private static readonly int hashAttackIndex = Animator.StringToHash("AttackIndex");
    private static readonly int hashShieldActive = Animator.StringToHash("ShieldActive");

    private int lastAttackIndex = 2; // empezamos en 2, así el primero será 1

    private void Awake()
    {
        if (sword != null)
            animator = sword.GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("WeaponController: No Animator found on sword object.");

        if (!shield)
            shield = GetComponentInParent<ShieldHandler>();

        if (shield != null)
        {
            shield.OnShieldActivated += HandleShieldActivated;
            shield.OnShieldDeactivated += HandleShieldDeactivated;
        }
    }

    private void OnDestroy()
    {
        if (shield != null)
        {
            shield.OnShieldActivated -= HandleShieldActivated;
            shield.OnShieldDeactivated -= HandleShieldDeactivated;
        }
    }

    private void HandleShieldActivated()
    {
        if (animator != null)
        {
            animator.SetBool(hashShieldActive, true);
            AudioManager.Instance.PlayOneShotSFX("Axe_Apper");
        }

    }

    private void HandleShieldDeactivated()
    {
        if (animator != null)
        {
            animator.SetBool(hashShieldActive, false);
            AudioManager.Instance.PlayOneShotSFX("Axe_Apper");

        }
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void PerformAttack()
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        if (animator != null)
        {
            // Alternar entre 1 y 2
            lastAttackIndex = (lastAttackIndex == 1) ? 2 : 1;

            animator.SetInteger(hashAttackIndex, lastAttackIndex);
            animator.SetTrigger(hashAttack);
            if (AudioManager.Exists)
            {
               AudioManager.Instance.PlayOneShotSFX(attackSfxNames[lastAttackIndex - 1]);
            }
        }
    }

    public float AttackCooldown => attackCooldown;
    /// <summary>
    /// Gets the hit delay for this weapon
    /// </summary>
    public float GetHitDelay()
    {
        if (hasCustomTiming)
        {
            return customHitDelay;
        }

        // Try to get timing from WeaponHitTiming component
        var hitTiming = GetComponent<WeaponHitTiming>();
        if (hitTiming != null)
        {
            return hitTiming.GetHitDelay();
        }

        // Fall back to default
        return 0.3f;
    }

    /// <summary>
    /// Sets custom hit delay for this weapon
    /// </summary>
    public void SetCustomHitDelay(float delay)
    {
        customHitDelay = delay;
        hasCustomTiming = true;
    }

    /// <summary>
    /// Enables or disables custom timing
    /// </summary>
    public void SetUseCustomTiming(bool useCustom)
    {
        hasCustomTiming = useCustom;
    }
}
