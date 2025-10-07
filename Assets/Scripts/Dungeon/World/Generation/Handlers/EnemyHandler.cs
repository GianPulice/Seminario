using System;
using System.Collections;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{
    [Header("Spawners de la sala")]
    [SerializeField] private EnemySpawner spawner;

    [Header("Rondas")]
    [Tooltip("Cantidad total de rondas en esta sala. Ejemplo: 3 --> (4, 8, 12 enemigos)")]
    [SerializeField] private int totalRounds = 3;
    [Tooltip("Número base de enemigos en la primera ronda. Cada ronda multiplica este número por el índice de ronda. Ejemplo: base = 4 --> ronda 1 = 4, ronda 2 = 8, ronda 3 = 12.")]
    [SerializeField] private int basePerRound = 2;  // 4, 8, 12...
    [SerializeField] private float timeBetweenRounds = 2f;

    public event Action OnAllEnemiesDefeated;

    private Coroutine roundsCoroutine;
    private RoomConfig roomConfig;
    private int currentLayer;
    private int aliveCount;
    private int currentRound;
    private bool initialized;

    public void Initialize(int layer,RoomConfig config)
    {
        currentLayer = layer;
        roomConfig = config;
        aliveCount = 0;
        currentRound = 0;
        initialized = true;

        if(roundsCoroutine != null)
            StopCoroutine(roundsCoroutine);
        roundsCoroutine = StartCoroutine(RunRounds());
    }

    public void Cleanup()
    {
        if (roundsCoroutine != null)
        {
            StopCoroutine(roundsCoroutine);
            roundsCoroutine = null;
        }
        initialized = false;
        aliveCount = 0;
    }

    private IEnumerator RunRounds()
    {
        if (!initialized || spawner == null || roomConfig == null)
        {
            Debug.LogWarning("[EnemyHandler] No hay spawners configurados en la sala.");
            yield break;
        }
        if (!initialized || spawner == null || roomConfig == null)
        {
            // LOG QUE ESPECIFICA EL FALLO
            string failReason = "";
            if (!initialized) failReason += " [Not Initialized]";
            if (spawner == null) failReason += " [Spawner NULL]";
            if (roomConfig == null) failReason += " [RoomConfig NULL]";

            Debug.LogWarning($"[EnemyHandler] Spawn detenido. Razón: {failReason}");
            yield break;
        }


        for (int round = 1; round <= totalRounds; round++)
        {
            currentRound = round;

            int baseEnemiesForLayer = roomConfig.GetEnemyCountForLayer(currentLayer);
            int toSpawn = Mathf.Max(1, baseEnemiesForLayer + currentRound);

            SpawnRound(toSpawn);

            yield return new WaitUntil(() => aliveCount <= 0);
            yield return new WaitForSeconds(timeBetweenRounds);
        }
        Debug.Log("[EnemyHandler] Todas las rondas completadas, invocando OnAllEnemiesDefeated.");
        OnAllEnemiesDefeated?.Invoke();
    }

    private void SpawnRound(int totalToSpawn)
    {
       if(totalToSpawn<=0 ||spawner==null) return;
        
        spawner.SpawnEnemies(roomConfig.roomID,totalToSpawn, currentLayer, OnEnemySpawned);
    }

    private void OnEnemySpawned(EnemyBase enemy)
    {
        if (enemy == null) return;

        aliveCount++;
        enemy.OnDeath -= HandleEnemyDeath;
        enemy.OnDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath(EnemyBase e)
    {
        e.OnDeath -= HandleEnemyDeath;
        aliveCount--;
        Debug.Log(aliveCount);
    }
}
