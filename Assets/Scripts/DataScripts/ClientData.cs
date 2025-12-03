using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientData", menuName = "ScriptableObjects/Tabern/Create New ClientData")]
public class ClientData : ScriptableObject
{
    [Header("AparienciaUI:")]
    [SerializeField] private Sprite clientImage;

    [Header("Parametros de comportamiento:")]
    [SerializeField] private float speed;
    [SerializeField] private float maxTimeWaitingForChair;
    [SerializeField] private float maxTimeWaitingToBeAttended;
    [SerializeField] private float maxTimeWaitingFood;
    [SerializeField] private float maxTimeEating;

    [Header("Propinas:")]
    [SerializeField] private List<GratuityOption> gratuityOptionsPercentage;
    [Range(0, 100)]
    [SerializeField] private float probabilityToGiveGratuity;

    public Sprite ClientImage { get => clientImage; }

    public float Speed { get => speed; }
    public float MaxTimeWaitingForChair { get => maxTimeWaitingForChair; }
    public float MaxTimeWaitingToBeAttended { get => maxTimeWaitingToBeAttended; }
    public float MaxTimeWaitingFood { get => maxTimeWaitingFood; }
    public float MaxTimeEating { get => maxTimeEating; }

    public List<GratuityOption> GratuityOptionsPercentage => gratuityOptionsPercentage;
    public float ProbabilityToGiveGratuity => probabilityToGiveGratuity;

    [Serializable]
    public class GratuityOption
    {
        [Range(0, 100)]
        [SerializeField] private int probability;
        [Range(0, 100)]
        [SerializeField] private int gratuityPercentage;

        public int Probability => probability;
        public int GratuityPercentage => gratuityPercentage;
    }
}
