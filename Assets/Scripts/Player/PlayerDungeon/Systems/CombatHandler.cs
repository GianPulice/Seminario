using UnityEngine;
using System.Collections;

public class CombatHandler : MonoBehaviour
{
    private PlayerDungeonModel model;
    private PlayerDungeonView view;
    private PlayerStamina stamina;

    [SerializeField] private AttackHitbox attackHitbox;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private ShieldHandler shieldHandler;

    [Header("Stamina Costs")]
    [SerializeField] private float attackStaminaCost = 20f;
    [Header("Hit Timing")]
    [SerializeField] private bool useHitDelay = true;
    [SerializeField] private float hitDelay = 0.3f;
    [SerializeField] private bool useWeaponTiming = true;


    private float lastAttackTime = -999f;
    private bool isAttacking;
    private Coroutine hitDelayCoroutine;

    private void Awake()
    {
        GetComponents();
    }

    public void TryAttack()
    {
        if (Time.time < lastAttackTime + model.AttackCooldown) return;
        if (shieldHandler != null && shieldHandler.IsActive) return;
        if (stamina == null || !stamina.CanUse(attackStaminaCost)) return;

        lastAttackTime = Time.time;
        stamina.Use(attackStaminaCost);

        isAttacking = true;
        view?.PlayAttackAnimation();
        weaponController?.PerformAttack();
        
        if (useHitDelay)
        {
            StartHitDelayCoroutine();
        }
        else
        {
            PerformHit();
        }

        Invoke(nameof(ResetAttack), model.AttackCooldown * 0.9f);
    }

    private void ResetAttack() => isAttacking = false;

    public void TryUseShield()
    {
        if (isAttacking) return;
        shieldHandler?.TryUseShield();
    }

    public void PerformHit()
    {
        attackHitbox?.TriggerHit();
    }

    private void StartHitDelayCoroutine()
    {
        // Stop any existing hit delay coroutine
        if (hitDelayCoroutine != null)
        {
            StopCoroutine(hitDelayCoroutine);
        }

        // Get the delay time (use weapon timing if available)
        float delayTime = GetHitDelayTime();

        // Start the hit delay coroutine
        hitDelayCoroutine = StartCoroutine(HitDelayCoroutine(delayTime));
    }

    private float GetHitDelayTime()
    {
        if (useWeaponTiming && weaponController != null)
        {
            // Get weapon-specific timing from WeaponController
            return weaponController.GetHitDelay();
        }

        // Fall back to default timing
        return hitDelay;
    }

    private IEnumerator HitDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        PerformHit();
        hitDelayCoroutine = null;
    }

    private void OnDestroy()
    {
        // Clean up coroutine if object is destroyed
        if (hitDelayCoroutine != null)
        {
            StopCoroutine(hitDelayCoroutine);
        }
    }

    private void GetComponents()
    {
        model = GetComponent<PlayerDungeonModel>();
        view = GetComponent<PlayerDungeonView>();
        attackHitbox = GetComponent<AttackHitbox>();
        weaponController = GetComponentInChildren<WeaponController>();
        shieldHandler = GetComponent<ShieldHandler>();
        stamina = GetComponent<PlayerStamina>();
    }

    public bool IsAttacking => isAttacking;
    public bool IsShieldActive => shieldHandler != null && shieldHandler.IsActive;
    public void SetHitDelay(float newDelay) => hitDelay = newDelay;
    public void SetUseHitDelay(bool useDelay) => useHitDelay = useDelay;
    public float GetCurrentHitDelay() => GetHitDelayTime();
}
