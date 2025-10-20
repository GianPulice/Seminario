using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Stats")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float healAmount = 10f;
    [SerializeField] private float healInterval = 30f;

    [Header("Debug")][SerializeField] private bool debugLogs = true;

    // Runtime
    private float currentHP;
    private bool isDead = false;
    private bool invulnerable = false;

    // Refs
    private PlayerKnockBack knockback;
    private ShieldHandler shieldHandler;

    public float CurrentHP => currentHP;
    public int MaxHP => maxHP;    
    public bool IsDead => isDead;

    //---Events---
    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;

    private void Awake()
    {
        currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
        StartCoroutine(AutoHeal());
        knockback = GetComponent<PlayerKnockBack>();
        shieldHandler = GetComponent<ShieldHandler>();
    }

    public void TakeDamage(int amount)
    {
        if (invulnerable || isDead) return;

        bool isEnemyDamage = DamageContext.Source == DamageSourceType.EnemyMelee || DamageContext.Source == DamageSourceType.EnemyProjectile;
        bool isBlocking = shieldHandler != null && shieldHandler.IsActive;

        if (isEnemyDamage && isBlocking)
        {
            if (debugLogs) Debug.Log($"[Shield] BLOQUEADO ({DamageContext.Source}) ? 0 daño");

            if (knockback != null)
            {
                Vector3 dir = DamageContext.HitDirection.sqrMagnitude > 0.0001f
                    ? DamageContext.HitDirection
                    : (transform.position - (DamageContext.Attacker ? DamageContext.Attacker.position : transform.position)).normalized;
                knockback.ApplyKnockback(dir, true); // knockback reducido
            }

            DamageContext.Clear();
            return;
        }

        // Daño normal
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);
        CombatFeedbackManager.Instance.PlayRandomDamageSound(transform.position);
        CombatFeedbackManager.Instance.ShakeCamera(0.15f, 0.25f, 5f);

        if (isEnemyDamage && knockback != null)
        {
            Vector3 dir = DamageContext.HitDirection.sqrMagnitude > 0.0001f
                ? DamageContext.HitDirection
                : (transform.position - (DamageContext.Attacker ? DamageContext.Attacker.position : transform.position)).normalized;
            knockback.ApplyKnockback(dir, false); // knockback normal
        }

        DamageContext.Clear();
        CheckDeath();

    }

    private void CheckDeath()
    {
        if (currentHP <= 0 && !isDead)
        {
            currentHP = 0;
            isDead = true;
            OnPlayerDied?.Invoke();
        }
    }

    private IEnumerator AutoHeal()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);

            if (!isDead && currentHP < maxHP)
            {
                currentHP = Mathf.Clamp(currentHP + healAmount, 0, maxHP);
                OnHealthChanged?.Invoke(currentHP, maxHP);
            }
        }
    }

    public void SetInvulnerable(bool value) => invulnerable = value;

    public void Revive()
    {
        currentHP = maxHP;
        isDead = false;
        SetInvulnerable(false);
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}
