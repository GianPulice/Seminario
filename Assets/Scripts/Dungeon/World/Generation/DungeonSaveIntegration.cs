using UnityEngine;

/// <summary>
/// Example integration script showing how to use the Memento-based save system
/// This should be attached to hallway spawn points or hallway controllers
/// </summary>
public class DungeonSaveIntegration : MonoBehaviour
{
    [Header("Hallway Configuration")]
    [SerializeField] private string hallwayId;
    [SerializeField] private bool isHallwaySpawn = true;

    private void Start()
    {
        // If this is a hallway spawn point, set up the hallway ID
        if (isHallwaySpawn && string.IsNullOrEmpty(hallwayId))
        {
            hallwayId = gameObject.name; // Use the GameObject name as hallway ID
        }
    }

    /// <summary>
    /// Call this method when the player enters this hallway
    /// This creates a Memento checkpoint with complete game state
    /// </summary>
    public void OnPlayerEnterHallway()
    {
        if (!isHallwaySpawn || string.IsNullOrEmpty(hallwayId))
        {
            Debug.LogWarning($"[DungeonSaveIntegration] Cannot create checkpoint - missing hallway ID or not a hallway spawn");
            return;
        }

        // Get current dungeon state
        var dungeonManager = DungeonManager.Instance;
        if (dungeonManager == null)
        {
            Debug.LogWarning("[DungeonSaveIntegration] DungeonManager not found");
            return;
        }

        // Get run history from DungeonManager
        var (roomsSinceLast, hallwaysSinceLast) = dungeonManager.GetCurrentRunHistory();

        // Create the Memento checkpoint
        HallwaySaveCoordinator.Instance?.OnEnterHallway(
            hallwayId,
            dungeonManager.CurrentLayer,
            transform.position,
            roomsSinceLast,
            hallwaysSinceLast
        );

        Debug.Log($"[DungeonSaveIntegration] Created Memento checkpoint at {hallwayId}");
    }

    /// <summary>
    /// Call this method when the player exits this hallway to enter a room
    /// This updates the session state but doesn't create a checkpoint
    /// </summary>
    public void OnPlayerExitToRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogWarning("[DungeonSaveIntegration] Cannot update session - missing room ID");
            return;
        }

        var dungeonManager = DungeonManager.Instance;
        if (dungeonManager != null)
        {
            HallwaySaveCoordinator.Instance?.OnEnterRoom(roomId, dungeonManager.CurrentLayer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Example: Auto-trigger checkpoint when player enters hallway
        if (other.CompareTag("Player") && isHallwaySpawn)
        {
            OnPlayerEnterHallway();
        }
    }

    // Example usage in code:
    /*
    // When player enters a hallway:
    DungeonSaveIntegration hallwaySave = hallwaySpawnPoint.GetComponent<DungeonSaveIntegration>();
    hallwaySave.OnPlayerEnterHallway();

    // When player exits hallway to enter a room:
    hallwaySave.OnPlayerExitToRoom("Room_1");
    */
}
