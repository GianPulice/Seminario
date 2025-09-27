using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapHandler : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefabs posibles de trampas que se pueden instanciar")]
    [SerializeField] private GameObject[] trapPrefabs;

    [Header("Spawnpoints")]
    [Tooltip("Posiciones fijas donde se pueden spawnear trampas en la sala")]
    [SerializeField] private Transform[] trapSpawnpoints;

    private GameObject[] spawnedTraps;
    public void SpawnTraps(RoomConfig config, int layer)
    {
        if (trapPrefabs.Length == 0 || trapSpawnpoints.Length == 0) return;

        int trapCount = Mathf.Min(GetTrapCountByRoomSize(config.size), trapSpawnpoints.Length);
        spawnedTraps = new GameObject[trapCount];

        Transform[] shuffledSpawnPoints = RouletteSelection.Shuffle((Transform[])trapSpawnpoints.Clone());


        for (int i = 0; i < trapCount; i++)
        {
            GameObject trapPrefab = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

            Transform spawnPoint = shuffledSpawnPoints[i];

            spawnedTraps[i] = Instantiate(trapPrefab, spawnPoint.position, spawnPoint.rotation, transform);
        }

        Debug.Log($"[TrapHandler] Spawned {trapCount} traps in {name}.");
    }
    public void Cleanup()
    {
        if (spawnedTraps == null) return;

        foreach (var trap in spawnedTraps)
        {
            if (trap != null)
                Destroy(trap);
        }

        spawnedTraps = null;
    }
    private int GetTrapCountByRoomSize(RoomSize size)
    {
        switch (size)
        {
            case RoomSize.Small: return 1;
            case RoomSize.Medium: return 2;
            case RoomSize.Large: return 3;
            default: return 1;
        }
    }
}
