using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public float money;

    public string lastSceneName;

    public List<IngredientStock> ingredientInventory = new List<IngredientStock>();

    public CheckpointData lastHallwayCheckpoint = new();
    public SessionFlags session = new();
}

//-------------------------TAVERN----------------------------

[Serializable]
public class IngredientStock
{
    public IngredientType ingredientType;
    public int amount;
}

//-------------------------DUNGEON----------------------------
[Serializable]
public class CheckpointData
{
    public string hallwayId;                 // e.g., "Hallway_3" from DungeonManager
    public int layer;                        // current layer at checkpoint
    public float scoreAtEntry;               // snapshot of Money when entering hallway
    public List<IngredientStock> ingredients = new();
    public SerializableVector3 playerPosition; // hallway-safe position
    public List<string> recentRooms = new(); // rooms visited since previous hallway
    public List<string> recentHallways = new();
    public DungeonRunHistory runHistory = new(); // Complete dungeon progression history
    public long timestampUtc;
}

[Serializable]
public class DungeonRunHistory
{
    public List<DungeonEvent> eventHistory = new(); // Complete history of rooms and hallways
    public int totalRoomsCompleted = 0;
    public int currentRunIndex = 0;
    public int totalRuns = 0;
}

[Serializable]
public class DungeonEvent
{
    public string eventType; // "Room" or "Hallway"
    public string eventId;   // Room/Hallway identifier
    public int layer;        // Layer when event occurred
    public bool wasCompleted; // For rooms: if they were cleared
    public long timestampUtc;
    public float moneyAtTime;
    public List<IngredientStock> ingredientsAtTime = new();
}

[Serializable]
public class SessionFlags
{
    public string currentLocation;           // "Hallway" | "Room"
    public string currentId;                 // id of room/hallway
    public bool pendingReturnToHallway;      // true if quit inside a room
    public int runIndex;                     // optional, for history
}
public struct SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    public Vector3 ToVector3() => new Vector3(x, y, z);
}