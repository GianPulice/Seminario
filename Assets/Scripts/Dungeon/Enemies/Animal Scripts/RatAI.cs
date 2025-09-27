using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class RatAI : EnemyBase
{
    [Header("Movimiento errático")]
    [SerializeField] private float circleRadius = 2f;
    [SerializeField] private float directionChangeInterval = 0.4f;
    [Space(2)]
    [Header("Audio")]
    [SerializeField] private AudioClip atkClip;

    private Vector3 currentOffset;
    private float dirTimer;

    private float attackCooldownTimer;
    private bool isAttacking;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemyData.Speed;
        agent.acceleration = 30f;
        agent.angularSpeed = 720f;
        agent.stoppingDistance = enemyData.DistanceToPlayer;
        agent.autoBraking = false;
        agent.isStopped = true;

        dirTimer = directionChangeInterval;
    }

    private void Update()
    {
        if (player == null || isAttacking) return;

        // ⬇️ Bloque nuevo: pausa durante el knockback
        var kb = GetComponent<EnemyKnockback>();
        if (kb != null && kb.IsActive)
        {
            agent.isStopped = true;
            return;
        }

        attackCooldownTimer += Time.deltaTime;

        PerceptionUpdate();

        if (canSeePlayer)
            ChasePlayer();
        else
            ErraticMove();
    }

    private void ChasePlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (dist <= enemyData.DistanceToPlayer)
        {
            agent.isStopped = true;
            TryAttack();
        }
    }

    private void ErraticMove()
    {
        agent.speed = enemyData.Speed * 1.2f;

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
        {
            Vector2 rnd = Random.insideUnitCircle.normalized * circleRadius;
            currentOffset = new Vector3(rnd.x, 0f, rnd.y);

            Vector3 targetPos = player.position + currentOffset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 1.0f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(player.position);

            dirTimer = directionChangeInterval;
        }

        agent.isStopped = false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= enemyData.DistanceToPlayer)
        {
            agent.isStopped = true;
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (attackCooldownTimer < enemyData.AttackCooldown) return;

        Vector3 hitDir = (player.position - transform.position).normalized; // desde atacante -> jugador
        DamageContext.Set(DamageSourceType.EnemyMelee, transform, player.position, hitDir);
        playerDamageable.TakeDamage(enemyData.Damage);
        DamageContext.Clear(); // opcional, por prolijidad
        audioSource.PlayOneShot(atkClip);

        attackCooldownTimer = 0f;
        StartCoroutine(AttackDelay());
    }

    private IEnumerator AttackDelay()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.DistanceToPlayer);
        LineOfSight.DrawLOSOnGizmos(transform, visionAngle, visionRange);
    }
#endif
}
