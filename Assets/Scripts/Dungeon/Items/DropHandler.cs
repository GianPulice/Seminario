
using System.Collections.Generic;
using UnityEngine;

public class DropHandler : MonoBehaviour
{
    [SerializeField] private DropTable table;
    [SerializeField] private LootPrefabDatabase lootDB;
    [SerializeField] private Transform spawnPoint;   // dónde cae el loot

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
        Init(table, lootDB, spawnPoint);
    }
    public void Init(DropTable t, LootPrefabDatabase db, Transform point)
    {
        table = t;
        lootDB = db;
        spawnPoint = point != null ? point : transform;
    }
    public void DropLoot(int currentLayer = 1)
    {
        if (table == null || lootDB == null || spawnPoint == null)
        {
            Debug.LogWarning("[DropHandler] Componentes no configurados correctamente.");
            return;
        }

        // Obtener los drops basados en la layer actual
        List<DropEntry> drops = table.GetDropsForLayer(currentLayer);

        foreach (var drop in drops)
        {
            if (Random.Range(0f, 100f) <= drop.probability * 100f)
            {
                Spawn(drop);
            }
        }
    }

    private void Spawn(DropEntry drop)
    {
        GameObject prefab = lootDB.GetLootPrefab(drop.lootName);
        if (prefab == null)
        {
            Debug.LogWarning($"Sin prefab para «{drop.lootName}»");
            return;
        }

        GameObject go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        // Inyección de datos si es ingrediente
        if (go.TryGetComponent(out IngredientPickup ing))
        {
            if (System.Enum.TryParse(drop.lootName, out IngredientType type))
            {
                ing.GetType().GetField("ingredient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(ing, type);
            }

            int qty = Random.Range(drop.minAmount, drop.maxAmount + 1);
            ing.GetType().GetField("amount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(ing, qty);
        }

        Debug.Log($"[DropHandler] Dropped {drop.lootName} (Layer {DungeonManager.Instance.CurrentLayer}) with chance {drop.probability * 100}%");
    }
}