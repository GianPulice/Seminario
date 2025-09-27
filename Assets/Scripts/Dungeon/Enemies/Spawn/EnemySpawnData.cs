using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public EnemyTypeSO enemyType; // Reference to SO
    [Range(0, 100)]
    public float spawnChance;
}

