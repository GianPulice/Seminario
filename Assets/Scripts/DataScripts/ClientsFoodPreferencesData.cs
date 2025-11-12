using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientsFoodPreferencesData", menuName = "ScriptableObjects/Tabern/Create New ClientsFoodPreferencesData")]
public class ClientsFoodPreferencesData : ScriptableObject
{
    [SerializeField] private List<FoodChances> foodChances;


    [Serializable]
    public class FoodChances
    {
        [SerializeField] private FoodType foodType;
        [Range(0, 100)]
        [SerializeField] private int probability;

        public FoodType FoodType { get => foodType; }
        public int Probability { get => probability; }
    }


    public FoodType? GetRandomFood()
    {
        var availableFoods = foodChances.FindAll(f => RecipeProgressManager.Instance.IsRecipeUnlocked(f.FoodType));

        if (availableFoods.Count == 0)
            return null;

        int totalProbability = 0;
        foreach (var food in availableFoods)
            totalProbability += food.Probability;

        int roll = UnityEngine.Random.Range(0, totalProbability);
        int cumulative = 0;

        foreach (var option in availableFoods)
        {
            cumulative += option.Probability;
            if (roll < cumulative)
                return option.FoodType;
        }

        // Si por alguna razón no salió en el roll, devolver el que tiene mayor probabilidad
        FoodChances highestProbabilityFood = null;
        int highestProb = int.MinValue;
        foreach (var food in availableFoods)
        {
            if (food.Probability > highestProb)
            {
                highestProb = food.Probability;
                highestProbabilityFood = food;
            }
        }

        return highestProbabilityFood.FoodType;
    }
}
