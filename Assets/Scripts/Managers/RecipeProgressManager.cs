using System.Collections.Generic;
using System;
using UnityEngine;

public class RecipeProgressManager : Singleton<RecipeProgressManager>
{
    [Header("Recetas desbloqueadas por defecto")]
    [SerializeField] private List<FoodRecipeData> defaultRecipes;

    [Header("Todas las recetas del juego")]
    [SerializeField] private List<FoodRecipeData> allRecipes;

    private HashSet<FoodType> unlockedRecipes = new HashSet<FoodType>();
    private HashSet<IngredientType> unlockedIngredients = new HashSet<IngredientType>();

    private event Action<FoodType> onRecipeUnlocked;
    private event Action<IngredientType> onIngredientUnlocked;

    public IReadOnlyList<FoodRecipeData> AllRecipes => allRecipes;

    public Action<FoodType> OnRecipeUnlocked { get => onRecipeUnlocked; set => onRecipeUnlocked = value; }
    public Action<IngredientType> OnIngredientUnlocked { get => onIngredientUnlocked; set => onIngredientUnlocked = value; }

    public bool IsRecipeUnlocked(FoodType type) => unlockedRecipes.Contains(type);
    public bool IsIngredientUnlocked(IngredientType type) => unlockedIngredients.Contains(type);


    void Awake()
    {
        CreateSingleton(false);
        UnlockRecipesAndIngredientsByDefault();
    }


    public void UnlockRecipe(FoodType type)
    {
        if (unlockedRecipes.Add(type))
        {
            onRecipeUnlocked?.Invoke(type);
        }
    }

    public void UnlockIngredient(IngredientType type)
    {
        if (unlockedIngredients.Add(type))
        {
            onIngredientUnlocked?.Invoke(type);
        }
    }


    private void UnlockRecipesAndIngredientsByDefault()
    {
        foreach (var recipeData in defaultRecipes)
        {
            UnlockRecipe(recipeData.FoodType);

            // Desbloquear automáticamente los ingredientes de esta receta
            foreach (var ingredientAmount in recipeData.Ingridients)
            {
                UnlockIngredient(ingredientAmount.IngredientType);
            }
        }
    }
}
