using UnityEngine;

public class PlayerDungeonView : MonoBehaviour
{
    private Animator animator;
    private PlayerDungeonModel model;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        model = GetComponent<PlayerDungeonModel>();
       
    }

    private void OnEnable()
    {
        if (model != null)
        {
            model.OnHealthChanged += HandleHealthChanged;
            model.OnStaminaChanged += HandleStaminaChanged;
            model.OnPlayerDied += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (model != null)
        {
            model.OnHealthChanged -= HandleHealthChanged;
            model.OnStaminaChanged -= HandleStaminaChanged;
            model.OnPlayerDied -= HandleDeath;
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        PlayerDungeonHUD.OnHealthChanged?.Invoke(current, max);
    }

    private void HandleStaminaChanged(float current, float max)
    {
        PlayerDungeonHUD.OnStaminaChanged?.Invoke(current, max);
    }

    private void HandleDeath()
    {
        animator?.SetTrigger("Die");
        PlayerDungeonHUD.OnPlayerDeath?.Invoke();
        DungeonManager.Instance?.OnPlayerDeath();
    }

    // Animaciones
    public void PlayAttackAnimation() => animator?.SetTrigger("Attack");
    public void OnAttackFrame() => GetComponent<AttackHitbox>()?.TriggerHit();

}
