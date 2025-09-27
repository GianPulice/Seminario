using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScreenManager : Singleton<DebugScreenManager>
{
    [System.Serializable]
    public class SpawnData
    {
        public string id;       // Identificador (ej: "SmallRoom1")
        public Transform point; // El Transform en escena
    }

    [SerializeField] private List<SpawnData> spawnPoints;
    [SerializeField] private Transform playerRef;
    private void Awake()
    {
        CreateSingleton(false);
    }
   
    public Transform GetSpawn(string id)
    {
        return spawnPoints.Find(s => s.id == id)?.point;
    }
}
    
