using System.Collections.Generic;
using UnityEngine;

public class LootHandler : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] lootSpawnpoints;

    [Header("Prefabs")]
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private GameObject chestPrefab;

    [Header("Layer Modifiers")]
    [SerializeField] private float lootMultiplierPerLayer = 0.1f; // 10% más loot por capa

    public void SpawnLoot(RoomSize roomSize, int currentLayer = 1)
    {
        if (lootSpawnpoints.Length == 0) return;

        int spawnCount = GetSpawnCount(roomSize, currentLayer);

        Transform[] shuffledPoints = RouletteSelection.Shuffle((Transform[])lootSpawnpoints.Clone());
       
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = shuffledPoints[i];
            GameObject prefabToSpawn = RollLootType(roomSize, currentLayer);

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity, transform);
            }
        }
    }

    private int GetSpawnCount(RoomSize roomSize, int currentLayer)
    {
        int baseCount = roomSize switch
        {
            RoomSize.Small => 2,
            RoomSize.Medium => 3,
            RoomSize.Large => 4,
            _ => 0
        };

        int extra = Mathf.Min(currentLayer / 2, 2); // bonus por capas
        return Mathf.Min(baseCount + extra, lootSpawnpoints.Length);
    }

    private GameObject RollLootType(RoomSize roomSize, int currentLayer)
    {
        // Chance base de cofre
        float chestChance = roomSize switch
        {
            RoomSize.Small => 0.20f,
            RoomSize.Medium => 0.30f,
            RoomSize.Large => 0.40f,
            _ => 0f
        };

        // Bonus por capa (máx 30%)
        float layerBonus = Mathf.Min(currentLayer * lootMultiplierPerLayer, 0.3f);
        chestChance += layerBonus;

        // Probabilidades en una ruleta
        var lootOptions = new Dictionary<GameObject, float>()
        {
            { chestPrefab, chestChance },
            { barrelPrefab, 1f - chestChance } // lo que sobra es barrel
        };

        return RouletteSelection.Roulette(lootOptions);
    }

    public void Cleanup()
    {
        // Destruir todos los objetos de loot hijos
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Loot"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}
