using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyType", menuName = "Dungeon/Enemy Type")]
public class EnemyTypeSO : ScriptableObject
{
    [Header("General")]
    public string enemyId; // Unique string for roulette and spawn table
    public string enemyName;
    public GameObject enemyPrefab;

    [Header("Stats")]
    public int HP = 100;
    public int Damage= 10;
    public float Speed= 3.5f;

    [Header("Combat")]
    [Tooltip("Distancia maxima (en metros) para intentar morder")]
    public float DistanceToPlayer = 3f;
    [Tooltip("Segundos entre mordiscos")]
    public float AttackCooldown = 1.5f;

    [Header("Loot")]
    public int minCoins = 1;
    public int maxCoins = 5;

    [TextArea] public string description;
} 