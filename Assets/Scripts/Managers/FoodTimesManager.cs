using System.Collections.Generic;
using UnityEngine;

public class FoodTimesManager : Singleton<FoodTimesManager>
{
    [SerializeField] private List<FoodData> foodsData;


    void Awake()
    {
        CreateSingleton(false);
    }

    // Funciona porque el nombre del ScriptableObject es el mismo que el del enum
    public FoodData GetFoodData(FoodType foodType)
    {
        return foodsData.Find(f => f.name == foodType.ToString());
    }
}
