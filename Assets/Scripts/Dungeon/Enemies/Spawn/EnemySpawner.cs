using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class RoomSpawnPoint
    {
        public string roomId;
        public List<Transform> points = new();
    }

    [Header("Refs")]
    [SerializeField] private AbstractFactory enemyFactory;
    [SerializeField] private List<ObjectPooler> enemyPools = new();
    [SerializeField] private StatScaler statScaler;
    [SerializeField] private EnemySpawnTableData spawnTableData;

    [Header("Spawn Points por habitación")]
    [SerializeField] private List<RoomSpawnPoint> spawnPoints = new();

    private Dictionary<string, ObjectPooler> enemyPoolDict = new();
    private bool isActive = false;


    public bool IsActive { get => isActive; set => isActive = value; }
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
                string id = pool.Prefab.name;
                if (!enemyPoolDict.ContainsKey(id))
                    enemyPoolDict[id] = pool;
            }
        }
    }

    public void SpawnEnemies(string roomId, int amount, int layer, Action<EnemyBase> onSpawned)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[EnemySpawner] Spawn cancelado. Active: {isActive}, Amount: {amount}.");
            return;
        }
       

        List<Transform> roomPoints = GetSpawnPointsForRoom(roomId);
        if (roomPoints == null || roomPoints.Count == 0)
        {
            Debug.LogWarning($"[EnemySpawner] No se encontraron spawn points para la room '{roomId}'.");
            return;
        }
        // Mezclamos los puntos de spawn
        List<Transform> shuffledPoints = RouletteSelection.Shuffle(roomPoints);

        for (int i = 0; i < amount; i++)
        {
            EnemyTypeSO enemyType = GetEnemyTypeForLayer(layer);
            if (enemyType == null)
            {
                Debug.LogWarning("[EnemySpawner] No se encontró un tipo de enemigo para esta capa.");
                continue;
            }

            Transform spawnPoint = shuffledPoints[i % shuffledPoints.Count];

            // Crear enemigo
            EnemyBase enemy = CreateEnemy(enemyType.enemyId, spawnPoint, layer);
            if (enemy == null) continue;

            // Vincular retorno al pool
            AttachReturnToPoolOnDeath(enemy,enemyType.enemyId);

            // Callback al Handler
            onSpawned?.Invoke(enemy);
        }
    }
    private void AttachReturnToPoolOnDeath(EnemyBase enemy,string enemyId)
    {
        if (!enemyPoolDict.TryGetValue(enemyId, out ObjectPooler pooler) || pooler == null)
        {
            Debug.LogWarning($"[EnemySpawner] No se encontró pooler para ID: {enemyId}.");
            return;
        }
       
        Action<EnemyBase> deathHandler = null;
        deathHandler = (e) =>
        {
            e.OnDeath -= deathHandler; // Limpiamos la suscripción
            pooler.ReturnObjectToPool(e); // Devolvemos al pooler capturado
        };

        // Suscribimos la función anónima a la muerte
        enemy.OnDeath += deathHandler;
    }

    private EnemyBase CreateEnemy(string enemyId, Transform spawnPoint, int layer)
    {
        GameObject obj = enemyFactory.CreateObject(enemyId, spawnPoint, Vector3.zero);
        if (obj == null)
        {
            Debug.LogWarning($"[EnemySpawner] No se pudo crear objeto para id '{enemyId}'");
            return null;
        }

        EnemyBase enemy = obj.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogWarning($"[EnemySpawner] El prefab '{enemyId}' no tiene EnemyBase.");
            return null;
        }

        statScaler?.ApplyScaling(enemy, layer);
        return enemy;
    }
    private List<Transform> GetSpawnPointsForRoom(string roomId)
    {
        RoomSpawnPoint room = spawnPoints.FirstOrDefault(r => r.roomId == roomId);
        return room?.points ?? new List<Transform>();
    }

    private EnemyTypeSO GetEnemyTypeForLayer(int layer)
    {
        if (spawnTableData == null || spawnTableData.tables.Count == 0)
            return null;

        layer = Mathf.Min(layer, 7); // límite máximo

        LayerSpawnTable table = spawnTableData.tables
            .OrderBy(t => t.layer)
            .FirstOrDefault(t => layer <= t.layer)
            ?? spawnTableData.tables.Last();

        if (table.spawnDataList.Count == 0) return null;

        var weighted = new Dictionary<EnemyTypeSO, float>();
        foreach (var data in table.spawnDataList)
            weighted[data.enemyType] = data.spawnChance;

        return RouletteSelection.Roulette(weighted);
    }
}
