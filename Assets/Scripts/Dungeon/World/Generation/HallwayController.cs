using UnityEngine;

public class HallwayController : MonoBehaviour
{
   [SerializeField] private DoorController[] doors;
    void Awake()
    {
        doors = GetComponentsInChildren<DoorController>();
    }
    private void Start()
    {
        if(doors != null && doors.Length > 0)
        {
            UnlockDoors();
        }
    }
    public void UnlockDoors()
    {
        foreach (var door in doors)
        {
            door.Unlock();
        }
       // Debug.Log($"[HallwayController] Se desbloquearon {doors.Length} puertas en el hallway {gameObject.name}");
    }
}
