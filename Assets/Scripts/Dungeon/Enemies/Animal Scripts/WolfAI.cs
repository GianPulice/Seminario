using UnityEngine;
using System.Collections;

public class WolfAI : EnemyBase
{
    [Header("Lobo")]
    [SerializeField] private float attackRange = 5f; // área de impacto del salto
    [SerializeField] private float leapDelay = 0.5f; // Pausa antes de saltar
    [Space(2)]
    [Header("Audio")]
    [SerializeField] private AudioClip atkClip;
    private float attackCooldownTimer = 0f;
    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        UpdateManager.OnUpdate += WoflUpdate;
    }

    private void OnDisable()
    {
        UpdateManager.OnUpdate -= WoflUpdate;
    }

    private void WoflUpdate()
    {
        if (IsDead || isAttacking) return;

        // Pausa durante el knockback
        var kb = GetComponent<EnemyKnockback>();
        if (kb != null && kb.IsActive)
        {
            agent.isStopped = true;
            return;
        }

        attackCooldownTimer += Time.deltaTime;

        PerceptionUpdate();

        // Siempre persigue al jugador si no está atacando
        if (player != null) // Añadido chequeo por si el jugador muere
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= attackRange && attackCooldownTimer >= enemyData.AttackCooldown && CanSeePlayer)
            {
                agent.isStopped = true;
                StartCoroutine(JumpAndBite());
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }
    private IEnumerator JumpAndBite()
    {
        isAttacking = true;
        yield return new WaitForSeconds(leapDelay);

        if (audioSource != null && atkClip != null)
        {
            audioSource.PlayOneShot(atkClip);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(enemyData.Damage);
        }

        attackCooldownTimer = 0f;
        isAttacking = false;
    }

    public override void ResetEnemy(Vector3 spawnPosition)
    {
        base.ResetEnemy(spawnPosition);

        attackCooldownTimer = 0f;
        isAttacking = false;

        if (agent != null)
        {
            agent.speed = enemyData.Speed;
            agent.isStopped = false;
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        LineOfSight.DrawLOSOnGizmos(transform, visionAngle, visionRange);
    }
#endif
}