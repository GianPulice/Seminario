using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Tooltip("Controla todas las rondas de enemigos de una sala. Coordina varios EnemySpawner y notifica al RoomController.")]
public class EnemyHandler : MonoBehaviour
{
    [Header("Spawners de la sala")]
    [Tooltip("Lista de EnemySpawner que se usan en la sala. Si se deja vacío, se auto-detectan en los hijos del Room.")]
    [SerializeField] private List<EnemySpawner> spawners = new();

    [Header("Rondas")]
    [Tooltip("Cantidad total de rondas en esta sala. Ejemplo: 3 --> (4, 8, 12 enemigos)")]
    [SerializeField] private int totalRounds = 3;

    [Tooltip("Número base de enemigos en la primera ronda. Cada ronda multiplica este número por el índice de ronda. Ejemplo: base = 4 --> ronda 1 = 4, ronda 2 = 8, ronda 3 = 12.")]
    [SerializeField] private int basePerRound = 2;  // 4, 8, 12...

    public event Action OnAllEnemiesDefeated;

    private Coroutine roundsCoroutine;
    private int currentLayer;
    private int aliveCount;
    private int currentRound;
    private bool initialized;

    public List<EnemySpawner> Spawners => spawners;

    public void Initialize(int layer)
    {
        currentLayer = layer;
        aliveCount = 0;
        currentRound = 0;
        initialized = true;

        if (spawners == null || spawners.Count == 0)
        {
            spawners = new List<EnemySpawner>(GetComponentsInChildren<EnemySpawner>(true));
        }

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
        foreach (var s in spawners)
            s?.ResetSpawner();

        initialized = false;
        aliveCount = 0;
        OnAllEnemiesDefeated = null;
    }

    private IEnumerator RunRounds()
    {
       
        if (!initialized || spawners == null || spawners.Count == 0)
        {
            Debug.LogWarning("[EnemyHandler] No hay spawners configurados en la sala.");
            yield break;
        }

        for (int round = 1; round <= totalRounds; round++)
        {
            currentRound = round;

            int toSpawn = basePerRound;

            int layerBonus = currentLayer / 3;
            toSpawn += layerBonus;
            

            if (toSpawn <= 0) toSpawn = 1; 

            Debug.Log($"[EnemyHandler] Iniciando Ronda {round}. Enemigos a spawnear: {toSpawn}");

            yield return SpawnRound(toSpawn);

            yield return new WaitUntil(() => aliveCount <= 0);
        }

        Debug.Log("[EnemyHandler] Todas las rondas completadas, invocando OnAllEnemiesDefeated.");
        OnAllEnemiesDefeated?.Invoke();
    }

    private IEnumerator SpawnRound(int totalToSpawn)
    {
        if (totalToSpawn <= 0) yield break;

        int spawnerCount = spawners.Count;
        int basePerSpawner = totalToSpawn / spawnerCount;
        int remainder = totalToSpawn % spawnerCount;

        var coroutines = new List<Coroutine>();

        for (int i = 0; i < spawnerCount; i++)
        {
            int countForThis = basePerSpawner + (i < remainder ? 1 : 0);
            if (countForThis <= 0) continue;

            var s = spawners[i];
            Coroutine c = StartCoroutine(s.SpawnEnemies(countForThis, currentLayer, OnEnemySpawned));
            coroutines.Add(c);
        }

        // Esperar a que todos los spawners terminen de soltar su tanda
        foreach (var c in coroutines)
            yield return c;
    }

    private void OnEnemySpawned(EnemyBase enemy)
    {
        if (enemy == null) return;

        aliveCount++;
        Debug.Log($"[EnemyHandler] Spawned {enemy.name}, vivos: {aliveCount}");

        enemy.OnDeath -= HandleEnemyDeath;
        enemy.OnDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath(EnemyBase e)
    {

        e.OnDeath -= HandleEnemyDeath;
        aliveCount--;
        Debug.Log($"[EnemyHandler] {e.name} murió. Vivos restantes: {aliveCount}");
    }
}
