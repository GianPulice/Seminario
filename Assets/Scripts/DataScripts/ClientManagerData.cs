using UnityEngine;

[CreateAssetMenu(fileName = "ClientManagerData", menuName = "ScriptableObjects/Tabern/Create New ClientManagerData")]
public class ClientManagerData : ScriptableObject
{
    [SerializeField] private int minimumOrdersServedToCloseTabern;
    [SerializeField] private float timeToWaitForSpawnNewClient;
    [SerializeField] private float delayToOpenTabernAgainAfterClose;

    public int MinimumOrdersServedToCloseTabern { get => minimumOrdersServedToCloseTabern; }
    public float TimeToWaitForSpawnNewClient { get => timeToWaitForSpawnNewClient; }
    public float DelayToOpenTabernAgainAfterClose { get => delayToOpenTabernAgainAfterClose; }
}
