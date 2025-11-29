using UnityEngine;

[CreateAssetMenu(fileName = "ClientData", menuName = "ScriptableObjects/Tabern/Create New ClientData")]
public class ClientData : ScriptableObject
{
    [SerializeField] private Sprite clientImage;

    [SerializeField] private float speed;
    [SerializeField] private float maxTimeWaitingForChair;
    [SerializeField] private float maxTimeWaitingToBeAttended;
    [SerializeField] private float maxTimeWaitingFood;
    [SerializeField] private float maxTimeEating;

    public Sprite ClientImage { get => clientImage; }

    public float Speed { get => speed; }
    public float MaxTimeWaitingForChair { get => maxTimeWaitingForChair; }
    public float MaxTimeWaitingToBeAttended { get => maxTimeWaitingToBeAttended; }
    public float MaxTimeWaitingFood { get => maxTimeWaitingFood; }
    public float MaxTimeEating { get => maxTimeEating; }
}
