using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientManagerData", menuName = "ScriptableObjects/Tabern/Create New ClientManagerData")]
public class ClientManagerData : ScriptableObject
{
    [SerializeField] private List<ClientSpawnChance> clientSpawnChances;
    [SerializeField] private List<FoodPayment> foodPayments;

    [SerializeField] private int minimumPaymentAmount;

    [SerializeField] private float minSpawnTime;
    [SerializeField] private float maxSpawnTime;
    [SerializeField] private float delayToFreeTableWhenClientLeaveTable;

    [Serializable]
    public class ClientSpawnChance
    {
        public ClientType clientType;
        [Range(0, 100)] public int probability;
    }

    [Serializable]
    public class FoodPayment
    {
        public FoodType FoodType;
        public int PaymentAmount;
    }

    public int MinimumPaymentAmount { get => minimumPaymentAmount; }

    public float MinSpawnTime { get => minSpawnTime; }
    public float MaxSpawnTime { get => maxSpawnTime; }
    public float DelayToFreeTableWhenClientLeaveTable { get => delayToFreeTableWhenClientLeaveTable; }


    public ClientType? GetRandomClient(List<ClientType> availableClientTypes)
    {
        var filteredChances = clientSpawnChances.FindAll(c => availableClientTypes.Contains(c.clientType));

        if (filteredChances.Count == 0)
            return null;

        int totalProbability = 0;
        foreach (var c in filteredChances)
            totalProbability += c.probability;

        int roll = UnityEngine.Random.Range(0, totalProbability);
        int cumulative = 0;

        foreach (var c in filteredChances)
        {
            cumulative += c.probability;
            if (roll < cumulative)
                return c.clientType;
        }

        // Si el roll falla, devolver el cliente con mayor probabilidad
        ClientSpawnChance highestProbabilityClient = null;
        int highestProb = int.MinValue;
        foreach (var c in filteredChances)
        {
            if (c.probability > highestProb)
            {
                highestProb = c.probability;
                highestProbabilityClient = c;
            }
        }

        return highestProbabilityClient.clientType;
    }

    public int GetPayment(FoodType foodType)
    {
        foreach (var fp in foodPayments)
        {
            if (fp.FoodType == foodType)
                return fp.PaymentAmount;
        }

        return 0;
    }
}
