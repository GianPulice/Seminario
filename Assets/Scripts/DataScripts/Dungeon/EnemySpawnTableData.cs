
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LayerSpawnTable
{
    public int layer;
    public List<EnemySpawnData> spawnDataList = new();
}

[CreateAssetMenu(fileName = "EnemySpawnTableData", menuName = "ScriptableObjects/Dungeon/EnemySpawnTableData")]
public class EnemySpawnTableData : ScriptableObject
{
    public List<LayerSpawnTable> tables = new();
}
