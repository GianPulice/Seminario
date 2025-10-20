using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootDatabase", menuName = "ScriptableObjects/Loot Prefab Database")]
public class LootPrefabDatabase : ScriptableObject
{
    // Una lista que contendrá las entradas del Editor.
    [System.Serializable]
    public class LootEntry
    {
        public string lootName;
        [Tooltip("Prefabs posibles dentro de esta categoría de loot.")]
        public List<GameObject> prefabs;
    }
    [Header("Categorías de Loot")]
    [SerializeField] private List<LootEntry> lootCategories; 

    private Dictionary<string, List<GameObject>> categoryLookup;
    /// <summary>
    /// Devuelve un prefab aleatorio de la categoría de loot solicitada.
    /// </summary>
    public GameObject GetLootPrefab(string lootName)
    {
        EnsureLookupBuilt();

        if (categoryLookup.TryGetValue(lootName, out List<GameObject> possiblePrefabs))
        {
            if (possiblePrefabs != null && possiblePrefabs.Count > 0)
            {
                int randomIndex = Random.Range(0, possiblePrefabs.Count);
                return possiblePrefabs[randomIndex];
            }
        }

        Debug.LogWarning($"[LootPrefabDatabase] No se encontró loot para '{lootName}' o la lista está vacía.");
        return null;
    }

    /// <summary>
    /// Construye el diccionario a partir de la lista serializada, si aún no está hecho.
    /// </summary>
    private void EnsureLookupBuilt()
    {
        if (categoryLookup != null) return;

        categoryLookup = new Dictionary<string, List<GameObject>>();

        foreach (var entry in lootCategories)
        {
            if (string.IsNullOrEmpty(entry.lootName)) continue;

            if (!categoryLookup.ContainsKey(entry.lootName))
                categoryLookup.Add(entry.lootName, entry.prefabs);
            else
                Debug.LogWarning($"[LootPrefabDatabase] Categoría duplicada: {entry.lootName}");
        }
    }
}