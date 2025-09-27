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
        public List<GameObject> prefabs;
    }

    [SerializeField] private List<LootEntry> lootCategories; 

    private Dictionary<string, List<GameObject>> categoryLookup;

    public GameObject GetLootPrefab(string lootName)
    {
        if (categoryLookup == null)
        {
            // Inicialización del diccionario si es nulo
            categoryLookup = new Dictionary<string, List<GameObject>>();
            foreach (var entry in lootCategories)
            {
                if (!categoryLookup.ContainsKey(entry.lootName))
                {
                    // Añade la categoría y su lista de prefabs
                    categoryLookup.Add(entry.lootName, entry.prefabs);
                }
            }
        }

        if (categoryLookup.TryGetValue(lootName, out List<GameObject> possiblePrefabs))
        {
            if (possiblePrefabs != null && possiblePrefabs.Count > 0)
            {
                int randomIndex = Random.Range(0, possiblePrefabs.Count);
                return possiblePrefabs[randomIndex];
            }
        }

        // Si no se encuentra la categoría o la lista está vacía
        return null;
    }
}