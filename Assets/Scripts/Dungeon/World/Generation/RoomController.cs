using System;
using System.Collections.Generic;
using UnityEngine;

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

    // ---------- EVENTS ----------
    public event Action OnAllEnemiesDefeated;

    //--------------- UNITY -------------------
    private void Awake()
    {
        InitializeHandlers();
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

        if (enemyHandler != null)
        {
            enemyHandler.Initialize(layer);
            enemyHandler.OnAllEnemiesDefeated += HandleRoomCleared;
        }

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

        if (enemyHandler != null)
        {
            enemyHandler.OnAllEnemiesDefeated -= HandleRoomCleared;
            enemyHandler.Cleanup();
        }

        lootHandler?.Cleanup();
        trapHandler?.Cleanup();
    }

    public void ResetRoom()
    {
        DeactivateRoom();
        InitializeHandlers();
    }
    private void InitializeHandlers()
    {
        if (enemyHandler == null)
        {
            enemyHandler = GetComponentInChildren<EnemyHandler>();
            if (enemyHandler == null)
                Debug.LogError($"[RoomController] EnemyHandler not found in children of {name}");
        }
        if (lootHandler == null)
        {
            lootHandler = GetComponentInChildren<LootHandler>();
            if (lootHandler == null)
                Debug.LogWarning($"[RoomController] LootHandler not found in children of {name}");
        }
        if (trapHandler == null)
        {
            trapHandler = GetComponentInChildren<TrapHandler>();
            if (trapHandler == null)
                Debug.LogWarning($"[RoomController] TrapHandler not found in children of {name}");
        }
    }
    private void HandleRoomCleared()
    {
        if (allEnemiesDefeated) return;

        allEnemiesDefeated = true;
        Debug.Log($"[RoomController] Sala {config.roomID} completada!");

        UnlockExitDoors();

        DungeonManager.Instance?.OnRoomCleared(this);
       
        OnAllEnemiesDefeated?.Invoke();
        OnAllEnemiesDefeated = null;
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
