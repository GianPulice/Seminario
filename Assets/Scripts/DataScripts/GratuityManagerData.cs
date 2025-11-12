using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GratuityManagerData", menuName = "ScriptableObjects/Tabern/Create New GratuityManagerData")]
public class GratuityManagerData : ScriptableObject
{
    [SerializeField] private List<GratuityOptions> gratuityOptionsPercentage;

    [Range(0, 100)]
    [SerializeField] private float probabilityToGiveGratuity;
    //[SerializeField] private float missedClientCost;
    [SerializeField] private float minimumPayment;

    public List<GratuityOptions> GratuityOptionsPercentage { get => gratuityOptionsPercentage; }

    public float ProbabilityToGiveGratuity { get => probabilityToGiveGratuity; }
    //public float MissedClientCost { get => missedClientCost; }
    public float MinimumPyament { get => minimumPayment; }


    [Serializable]
    public class GratuityOptions
    {
        [Range(0, 100)]
        [SerializeField] private int probability; 
        [Range(0, 100)]
        [SerializeField] private int gratuityPercentage; 

        public int Probability { get => probability; }
        public int GratuityPercentage { get => gratuityPercentage; }
    }
}
