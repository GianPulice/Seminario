using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(DropHandler))]
public class Barrel : MonoBehaviour, IDamageable
{
    [Header("Properties")]
    [SerializeField] private int hitPoints = 1;

    [Header("Loot System")]
    [SerializeField] private DropTable dropTable;
    [SerializeField] private LootPrefabDatabase lootDB;
    [SerializeField] private Transform lootSpawnPoint;

    private DropHandler dropHandler;

    private void Awake()
    {
        dropHandler = GetComponent<DropHandler>();
        dropHandler.Init(dropTable, lootDB, lootSpawnPoint != null ? lootSpawnPoint : transform);
    }

    public void TakeDamage(int value)
    {
        Debug.Log("Hice daño");
        TakeHit(value);
    }

    public void TakeHit(int value)
    {
        hitPoints -= value;
        if (hitPoints <= 0)
        {
            BreakBarrel();
        }
    }

    private void BreakBarrel()
    {
        Debug.Log("Barrel is breaking!");

        int currentLayer = DungeonManager.Instance.CurrentLayer;
        dropHandler.DropLoot(currentLayer);

        Destroy(gameObject);
    }
}
