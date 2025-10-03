using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AbstractFactory enemyFactory;
    [SerializeField] private List<ObjectPooler> enemyPools;
    [SerializeField] private StatScaler statScaler;
    [SerializeField] private EnemySpawnTableData spawnTableData;
    [Header("Spawnpoints")]
    [SerializeField] private List<Transform> spawnPoints = new();

    private Dictionary<string, ObjectPooler> enemyPoolDict = new();
    private bool isActive = false;


    public bool IsActive { get => isActive; set => isActive = value; }
    public Transform SpawnPosition => spawnPoints.Count > 0 ? spawnPoints[0] : null;
    void Awake()
    {
        InitializeEnemyPoolDictionary();
    }

    private void InitializeEnemyPoolDictionary()
    {
        foreach (var pool in enemyPools)
        {
            if (pool.Prefab != null)
            {
                enemyPoolDict[pool.Prefab.name] = pool;
            }
        }
    }

    /// <summary>
    /// Spawnea N enemigos según la capa (layer) y tabla de spawn.
    /// </summary>
    public void SpawnEnemies(int amount, int layer, Action<EnemyBase> onSpawned)
    {
        if (!isActive || spawnPoints == null || spawnPoints.Count == 0 || amount <= 0) return;
        List<Transform> shuffledPoints = RouletteSelection.Shuffle(new List<Transform>(spawnPoints));
        for(int i = 0;i<amount; i++)
        {
            Transform point = shuffledPoints[i % shuffledPoints.Count];
            EnemyTypeSO enemyType = GetEnemyTypeForLayer(layer);
            if (enemyType == null) continue;

            // Crear o recuperar del pool
            GameObject obj = enemyFactory.CreateObject(enemyType.enemyId);
            if (obj == null)
            {
                Debug.LogWarning($"[EnemySpawner] No se pudo crear objeto para id '{enemyType.enemyId}'");
                continue;
            }

            var enemy = obj.GetComponent<EnemyBase>();
            if (enemy == null)
            {
                Debug.LogWarning($"[EnemySpawner] El prefab '{enemyType.enemyId}' no tiene EnemyBase.");
                continue;
            }

            // Colocar y escalar
            enemy.transform.SetPositionAndRotation(point.position, point.rotation);
            statScaler?.ApplyScaling(enemy, layer);

            // Suscribir retorno al pool al morir
            if (enemyPoolDict.TryGetValue(enemyType.enemyId, out var pooler) && pooler != null)
            {
                Action<EnemyBase> deathHandler = null;
                deathHandler = (e) =>
                {
                    e.OnDeath -= deathHandler;
                    pooler.ReturnObjectToPool(e);
                };
                enemy.OnDeath += deathHandler;
            }

            // Avisar al handler
            onSpawned?.Invoke(enemy);
        }
    }

    private EnemyTypeSO GetEnemyTypeForLayer(int layer)
    {
        if (spawnTableData == null || spawnTableData.tables.Count == 0) return null;

        if (layer >= 7) layer = 7;

        LayerSpawnTable table = spawnTableData.tables
            .OrderBy(t => t.layer)
            .FirstOrDefault(t => layer <= t.layer);

        if (table == null) table = spawnTableData.tables.Last();

        if (table.spawnDataList.Count == 0) return null;

        var weighted = new Dictionary<EnemyTypeSO, float>();
        foreach (var data in table.spawnDataList)
            weighted[data.enemyType] = data.spawnChance;

        return RouletteSelection.Roulette(weighted);
    }
}
