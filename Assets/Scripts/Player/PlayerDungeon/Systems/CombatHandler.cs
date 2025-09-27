using UnityEngine;

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

    private float lastAttackTime = -999f;
    private bool isAttacking;

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
        PerformHit();

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
}
