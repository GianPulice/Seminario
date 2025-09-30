using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private EnemySpawnTableData spawnTableData;
    private Dictionary<string, GameObject> enemiesDictionary;

    private void Awake()
    {
        enemiesDictionary = new Dictionary<string, GameObject>();
        foreach (var table in spawnTableData.tables)
        {
            foreach (var spawnData in table.spawnDataList)
            {
                var enemyType = spawnData.enemyType;
                if (enemyType == null || string.IsNullOrEmpty(enemyType.enemyId)) continue;
                if (!enemiesDictionary.ContainsKey(enemyType.enemyId))
                    enemiesDictionary.Add(enemyType.enemyId, enemyType.enemyPrefab);
                else
                    Debug.LogWarning($"[EnemyFactory] Duplicate Id: {enemyType.enemyId}");
            }
        }
    }

    public EnemyBase Create(string id, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (enemiesDictionary != null && enemiesDictionary.TryGetValue(id, out GameObject prefab))
        {
            var instance = Instantiate(prefab, spawnPosition, spawnRotation,this.transform);
            return instance.GetComponent<EnemyBase>();
        }
        Debug.LogWarning($"[EnemyFactory] No prefab found with Id '{id}'");
        return null;
    }

    public GameObject GetPrefab(string id)
    {
        if (enemiesDictionary != null && enemiesDictionary.TryGetValue(id, out GameObject prefab))
            return prefab;
        return null;
    }

}
