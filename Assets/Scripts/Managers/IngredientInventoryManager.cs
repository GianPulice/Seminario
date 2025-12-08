using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientInventoryManager : Singleton<IngredientInventoryManager>
{
    private List<IngredientType> availableIngredients;
    [SerializeField] private IngredientInventoryManagerData ingredientInventoryManagerData;
    [SerializeField] private List<FoodRecipeData> foodRecipesData;
    [SerializeField] private List<IngredientData> ingredientsData;

    private Dictionary<IngredientType, int> ingredientInventory = new();
    private Dictionary<FoodType, FoodRecipeData> recipeDict = new();
    private Dictionary<IngredientType, IngredientData> ingredientDataDict = new();
    
    public List<IngredientData> IngredientsData { get => ingredientsData; }

    public Dictionary<IngredientType, IngredientData> IngredientDataDict { get =>  ingredientDataDict; }


    void Awake()
    {
        CreateSingleton(true);
        SuscribeToGameManagerEvent();
        InitializeIngredientData();
        InitializeRecipes();
    }


    public void IncreaseIngredientStock(IngredientType ingredient, int amount)
    {
        if (!ingredientInventory.ContainsKey(ingredient))
            ingredientInventory[ingredient] = 0;

        ingredientInventory[ingredient] += amount;
        SaveInventory();
    }

    public void IncreaseIngredientStock(IngredientType ingredient)
    {
        IncreaseIngredientStock(ingredient, 1);
        SaveInventory();
    }

    public bool TryCraftFood(FoodType foodType)
    {
        if (!recipeDict.ContainsKey(foodType)) return false;

        var recipe = recipeDict[foodType];

        foreach (var ing in recipe.Ingridients)
        {
            if (!ingredientInventory.ContainsKey(ing.IngredientType) || ingredientInventory[ing.IngredientType] < ing.Amount)
            {
                return false;
            }
        }

        foreach (var ing in recipe.Ingridients)
        {
            ingredientInventory[ing.IngredientType] -= ing.Amount;
        }

        SaveInventory();

        return true;
    }

    public int GetPriceOfIngredient(IngredientType ingredient)
    {
        return ingredientDataDict.TryGetValue(ingredient, out var data) ? data.Price : 0;
    }

    public int GetStock(IngredientType ingredient)
    {
        return ingredientInventory.TryGetValue(ingredient, out var stock) ? stock : 0;
    }

    public void SetStock(IngredientType ingredient, int amount)
    {
        if (amount < 0) amount = 0;
        ingredientInventory[ingredient] = amount;
        SaveInventory();
    }

    public FoodRecipeData GetRecipe(FoodType foodType)
    {
        return recipeDict.TryGetValue(foodType, out var recipe) ? recipe : null;
    }

    public List<IngredientType> GetAllIngredients() => new List<IngredientType>(ingredientInventory.Keys);
    
    public List<FoodRecipeData> GetAllRecipes()
    {
        return recipeDict.Values.ToList();
    }


    private void SuscribeToGameManagerEvent()
    {
        GameManager.Instance.OnGameSessionStarted += OnInitializeInventory;
    }

    private void OnInitializeInventory()
    {
        /*if (GameManager.Instance.GameSessionType == GameSessionType.Load && SaveSystemManager.SaveExists())
        {
            SaveData data = SaveSystemManager.LoadGame();

            ingredientInventory.Clear();

            foreach (var stock in data.ingredientInventory)
            {
                ingredientInventory[stock.ingredientType] = stock.amount;
            }
        }

        else
        {
            if (ingredientInventoryManagerData.IngredientsUseOwnStock)
            {
                foreach (var ingredientData in ingredientsData)
                {
                    var type = ingredientData.IngredientType;
                    var stock = ingredientData.InitializeStock;
                    ingredientInventory[type] = stock;
                }
            }

            else
            {
                availableIngredients = Enum.GetValues(typeof(IngredientType)).Cast<IngredientType>().ToList();

                foreach (IngredientType ingredient in availableIngredients)
                {
                    ingredientInventory[ingredient] = ingredientInventoryManagerData.InitializeStockForAllIngredients;
                }
            }
        }*/

        //SaveInventory();
    }

    private void SaveInventory()
    {
        /*SaveData data = SaveSystemManager.LoadGame();

        data.ingredientInventory.Clear();

        foreach (var kvp in ingredientInventory)
        {
            data.ingredientInventory.Add(new IngredientStock
            {
                ingredientType = kvp.Key,
                amount = kvp.Value
            });
        }

        SaveSystemManager.SaveGame(data);*/
    }

    private void InitializeIngredientData()
    {
        foreach (var data in ingredientsData)
        {
            if (!ingredientDataDict.ContainsKey(data.IngredientType))
            {
                ingredientDataDict[data.IngredientType] = data;
            }
        }
    }

    private void InitializeRecipes()
    {
        recipeDict = foodRecipesData.ToDictionary(r => r.FoodType, r => r);
    }
}