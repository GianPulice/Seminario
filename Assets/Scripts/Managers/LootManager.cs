using UnityEngine;

public class LootManager : Singleton<LootManager>
{
    [SerializeField] private LootPrefabDatabase lootDatabase;

    private void Awake()
    {
        CreateSingleton(false);
    }

    public void SpawnLoot(string lootName, Vector3 position)
    {
        GameObject prefab = lootDatabase.GetLootPrefab(lootName);
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[LootManager] Loot '{lootName}' no registrado en la base.");
        }
    }
}
