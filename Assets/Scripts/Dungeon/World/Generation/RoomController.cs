using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomController : MonoBehaviour
{
    [Header("Room Configuration")]
    [SerializeField] private RoomConfig config;

    [Header("Handlers")]
    [SerializeField] private EnemyHandler enemyHandler;
    [SerializeField] private LootHandler lootHandler;
    [SerializeField] private TrapHandler trapHandler;

    [Header("Doors")]
    [SerializeField] private DoorController entryDoor;
    [SerializeField] private DoorController[] exitDoors;

    [Header("References")]
    [SerializeField] private Transform roomSpawnPoint;

    private bool isActive = false;
    private bool allEnemiesDefeated = false;

    // ---------- PROPERTIES ----------
    public RoomConfig Config => config;
    public bool IsActive => isActive;
    public bool IsCleared => allEnemiesDefeated;
    public Transform SpawnPoint => roomSpawnPoint;

    //--------------- UNITY -------------------
    private void Awake()
    {
       if(enemyHandler != null)
            enemyHandler.OnAllEnemiesDefeated += HandleRoomCleared;
    }
    private void OnDestroy()
    {
        if (enemyHandler != null)
            enemyHandler.OnAllEnemiesDefeated -= HandleRoomCleared;
    }
    //------------------ API ------------------

    public void ActivateRoom()
    {
        if (isActive) return;
       
        isActive = true;
        allEnemiesDefeated = false;

        int layer = DungeonManager.Instance.CurrentLayer;

        Debug.Log($"[RoomController] Activando sala {config.roomID} en layer {layer}");

        enemyHandler?.Cleanup();
        enemyHandler?.Initialize(layer, config);

        if (config.allowLoot)
            lootHandler?.SpawnLoot(config.size, layer);

        if (config.allowTraps)
            trapHandler?.SpawnTraps(config, layer);

        LockExitDoors();
    }
    public void DeactivateRoom()
    {
        if (!isActive) return;

        isActive = false;

        enemyHandler?.Cleanup();
        lootHandler?.Cleanup();
        trapHandler?.Cleanup();
    }

    public void ResetRoom()
    {
        DeactivateRoom();
    }
    private void HandleRoomCleared()
    {
        if (allEnemiesDefeated) return;

        allEnemiesDefeated = true;
        Debug.Log($"[RoomController] Sala {config.roomID} completada!");

        UnlockExitDoors();

        DungeonManager.Instance?.OnRoomCleared(this);
    }

    private void LockExitDoors()
    {
        foreach (var door in exitDoors)
            door?.Lock();
    }

    private void UnlockExitDoors()
    {
        foreach (var door in exitDoors)
            door?.Unlock();
    }

}
