
using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Componente principal del enemigo. Gestiona HP, muerte
/// y delega el comportamiento a RatAI.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour,IDamageable
{
    [Header("Data")]
    public EnemyTypeSO enemyData;
    [SerializeField] private string id;
    private EnemyTypeSO originalData;
    public string Id => id;

    [Header("LoS")]
    [SerializeField] protected float visionRange = 8f;
    [SerializeField] protected float visionAngle = 100f;
    [SerializeField] protected LayerMask obstacleMask;
    [SerializeField] protected float chaseSpeedMultiplier = 1.5f;
    [SerializeField] protected float loseSightDelay = 1.5f;
   
    [Header("Runtime")]
    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    //public static int GlobalDeathCount = 0;
    //---References---
    protected DropHandler dropHandler;
    protected DamageFlash damageFlash;
    protected NavMeshAgent agent;
    protected IDamageable playerDamageable;
    protected Transform player;
    protected AudioSource audioSource;

    protected bool canSeePlayer = false;
    protected float loseSightTimer = 0f;

    public bool CanSeePlayer => canSeePlayer;

    public event Action<EnemyBase> OnDeath;

    protected virtual void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"[{name}] Missing EnemyData.");
            enabled = false;
            return;
        }
        originalData = enemyData;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerDamageable = playerObj.GetComponent<IDamageable>();
        }

        agent = GetComponent<NavMeshAgent>();
        damageFlash = GetComponent<DamageFlash>();
        dropHandler = GetComponent<DropHandler>();
        audioSource = GetComponent<AudioSource>();

        CurrentHP = enemyData.HP;
        IsDead = false;
    }
    public virtual void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHP -= amount;
        damageFlash?.TriggerFlash();
        SpawnBloodDecal();

        if (CurrentHP <= 0)
            Die();
    }
    public void SetScaledStats(float hpMul, float dmgMul, float speedMul)
    {
        enemyData = Instantiate(originalData);

        enemyData.HP= Mathf.RoundToInt(enemyData.HP * (1 + hpMul));
        enemyData.Damage = Mathf.RoundToInt(enemyData.Damage * (1 + dmgMul));
        enemyData.Speed *= (1 + speedMul);

        CurrentHP = enemyData.HP;
        agent.speed = enemyData.Speed;
    }
    protected virtual void Die()
    {
        if (IsDead) return;

        IsDead = true;
        agent.isStopped = true;

        //GlobalDeathCount++;
        //Debug.Log($"[EnemyBase] {name} murió. Total muertes: {GlobalDeathCount}");

        dropHandler?.DropLoot();
        SpawnBloodDecal();

        OnDeath?.Invoke(this);
    }

    protected void PerceptionUpdate()
    {
        if (player == null) return;

        bool seesPlayer = LineOfSight.LOS(transform, player, visionRange, visionAngle, obstacleMask);

        if (seesPlayer)
        {
            loseSightTimer = 0f;
            if (!canSeePlayer)
            {
                canSeePlayer = true;
                agent.speed = enemyData.Speed * chaseSpeedMultiplier;
            }
        }
        else
        {
            loseSightTimer += Time.deltaTime;
            if (canSeePlayer && loseSightTimer >= loseSightDelay)
            {
                canSeePlayer = false;
                agent.speed = enemyData.Speed;
                loseSightTimer = 0f;
            }
        }
    }

    public virtual void ResetEnemy()
    {
        CurrentHP = enemyData.HP;
        IsDead = false;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.speed = enemyData.Speed;
        }
        canSeePlayer = false;
        loseSightTimer = 0f;
    }

    private void SpawnBloodDecal()
    {
        if (BloodDecalManager.Instance == null) return;

        Vector3 origin = transform.position + Vector3.up * 1f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("whatIsGround")))
        {
            Vector3 spawnPos = hit.point + Vector3.up * 0.01f;
            Quaternion rot = Quaternion.Euler(90f, UnityEngine.Random.Range(0f, 360f), 0f);
            BloodDecalManager.Instance.Spawn(spawnPos, rot);
        }
    }

}
