using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HallwaySaveCoordinator : Singleton<HallwaySaveCoordinator>
{
    private void Awake()
    {
        CreateSingleton(true);
        DontDestroyOnLoad(gameObject);
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        GameManager.Instance.OnGameSessionStarted += OnSessionStart;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameSessionStarted -= OnSessionStart;
    }

    private void OnSessionStart()
    {
        // If coming from a prior run and pending flag set while being in a room, force place in last hallway
        //if (SaveSystemManager.SaveExists())
        {
            /*SaveData data = SaveSystemManager.LoadGame();

            if (data.session.pendingReturnToHallway && data.lastHallwayCheckpoint != null)
            {
                Debug.Log("[HallwaySaveCoordinator] Restoring from hallway checkpoint due to room exit");

                // Restore from checkpoint
                var cp = data.lastHallwayCheckpoint;
                TryPlacePlayerAt(cp.playerPosition.ToVector3());
                RestoreInventory(cp.ingredients);

                // Reset money to checkpoint value
                if (MoneyManager.Instance != null)
                {
                    MoneyManager.Instance.SubMoney(MoneyManager.Instance.CurrentMoney);
                    MoneyManager.Instance.AddMoney(cp.scoreAtEntry);
                }

                // Clear pending flag and update session
                data.session.currentLocation = "Hallway";
                data.session.currentId = cp.hallwayId;
                data.session.pendingReturnToHallway = false;

                //SaveSystemManager.SaveGame(data);
            }*/
        }
    }

    /// <summary>
    /// Call this when the player arrives to a hallway spawn - Creates a Memento checkpoint
    /// </summary>
    public void OnEnterHallway(string hallwayId, int layer, Vector3 hallwaySpawn, List<string> roomsSinceLast, List<string> hallwaysSinceLast)
    {
        Debug.Log($"[HallwaySaveCoordinator] Creating Memento checkpoint at hallway {hallwayId}");

        /*SaveData data = SaveSystemManager.LoadGame();

        // Create the Memento checkpoint with complete state
        var checkpoint = CreateHallwayMemento(hallwayId, layer, hallwaySpawn, roomsSinceLast, hallwaysSinceLast, data);

        data.lastHallwayCheckpoint = checkpoint;
        data.session.currentLocation = "Hallway";
        data.session.currentId = hallwayId;
        data.session.pendingReturnToHallway = false;*/

        //SaveSystemManager.SaveGame(data);
    }

    /// <summary>
    /// Creates a Memento checkpoint containing the complete game state
    /// </summary>
    /*private CheckpointData CreateHallwayMemento(string hallwayId, int layer, Vector3 hallwaySpawn, 
        List<string> roomsSinceLast, List<string> hallwaysSinceLast, SaveData currentData)
    {
        var checkpoint = new CheckpointData
        {
            hallwayId = hallwayId,
            layer = layer,
            scoreAtEntry = MoneyManager.Instance != null ? MoneyManager.Instance.CurrentMoney : 0f,
            ingredients = SnapshotCurrentIngredients(),
            playerPosition = new SerializableVector3(hallwaySpawn),
            recentRooms = new List<string>(roomsSinceLast),
            recentHallways = new List<string>(hallwaysSinceLast),
            runHistory = new DungeonRunHistory(),
            timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Copy existing run history and add new events
        if (currentData.lastHallwayCheckpoint?.runHistory != null)
        {
            checkpoint.runHistory = currentData.lastHallwayCheckpoint.runHistory;
        }

        // Add hallway event to history
        AddEventToHistory(checkpoint.runHistory, "Hallway", hallwayId, layer, true);

        return checkpoint;
    }*/

    /// <summary>
    /// Adds an event to the dungeon run history
    /// </summary>
    /*private void AddEventToHistory(DungeonRunHistory history, string eventType, string eventId, int layer, bool wasCompleted)
    {
        var dungeonEvent = new DungeonEvent
        {
            eventType = eventType,
            eventId = eventId,
            layer = layer,
            wasCompleted = wasCompleted,
            timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            moneyAtTime = MoneyManager.Instance != null ? MoneyManager.Instance.CurrentMoney : 0f,
            ingredientsAtTime = SnapshotCurrentIngredients()
        };

        history.eventHistory.Add(dungeonEvent);

        if (eventType == "Room" && wasCompleted)
        {
            history.totalRoomsCompleted++;
        }
    }*/

    /// <summary>
    /// Mark leaving hallway into room, but do not save a checkpoint here
    /// </summary>
    public void OnEnterRoom(string roomId, int layer)
    {
        /*SaveData data = SaveSystemManager.LoadGame();
        data.session.currentLocation = "Room";
        data.session.currentId = roomId;
        
        // Add room entry to history if we have a checkpoint
        if (data.lastHallwayCheckpoint?.runHistory != null)
        {
            AddEventToHistory(data.lastHallwayCheckpoint.runHistory, "Room", roomId, layer, false);
            //SaveSystemManager.SaveGame(data);
        }*/

        Debug.Log($"[HallwaySaveCoordinator] Entered room {roomId} - no checkpoint saved, but added to history");
    }

    /// <summary>
    /// Called when a room is completed - updates history
    /// </summary>
    public void OnRoomCompleted(string roomId, int layer)
    {
        /*SaveData data = SaveSystemManager.LoadGame();
        
        if (data.lastHallwayCheckpoint?.runHistory != null)
        {
            // Find the most recent room entry and mark it as completed
            var roomEvent = data.lastHallwayCheckpoint.runHistory.eventHistory
                .LastOrDefault(e => e.eventType == "Room" && e.eventId == roomId && !e.wasCompleted);
            
            if (roomEvent != null)
            {
                roomEvent.wasCompleted = true;
                data.lastHallwayCheckpoint.runHistory.totalRoomsCompleted++;
            }
            
            //SaveSystemManager.SaveGame(data);
            Debug.Log($"[HallwaySaveCoordinator] Room {roomId} marked as completed in history");
        }*/
    }

    /// <summary>
    /// Called by quit flow if the player is currently inside a room
    /// </summary>
    public void MarkPendingReturnToHallway()
    {
        //SaveData data = SaveSystemManager.LoadGame();
        //data.session.pendingReturnToHallway = (data.session.currentLocation == "Room");
        //SaveSystemManager.SaveGame(data);

        //Debug.Log($"[HallwaySaveCoordinator] Marked pending return to hallway: {data.session.pendingReturnToHallway}");
    }

    private void TryPlacePlayerAt(Vector3 pos)
    {
        var dm = DungeonManager.Instance;
        if (dm != null && dm.Player != null)
        {
            dm.Player.position = pos;
            Debug.Log($"[HallwaySaveCoordinator] Placed player at checkpoint position: {pos}");
        }
    }

    private List<IngredientStock> SnapshotCurrentIngredients()
    {
        var list = new List<IngredientStock>();
        var inv = IngredientInventoryManager.Instance;
        if (inv == null) return list;

        foreach (var ing in inv.GetAllIngredients())
        {
            list.Add(new IngredientStock { ingredientType = ing, amount = inv.GetStock(ing) });
        }
        return list;
    }

    private void RestoreInventory(List<IngredientStock> stocks)
    {
        var inv = IngredientInventoryManager.Instance;
        if (inv == null) return;

        Debug.Log($"[HallwaySaveCoordinator] Restoring inventory from checkpoint");

        // Clear current inventory by setting all to 0
        foreach (var ingredientData in inv.IngredientsData)
        {
            inv.SetStock(ingredientData.IngredientType, 0);
        }

        // Restore checkpoint inventory
        foreach (var stock in stocks)
        {
            inv.SetStock(stock.ingredientType, stock.amount);
        }

        Debug.Log($"[HallwaySaveCoordinator] Inventory restored successfully");
    }
}