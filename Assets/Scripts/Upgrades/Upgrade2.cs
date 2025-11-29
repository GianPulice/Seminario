using System.Collections.Generic;
using UnityEngine;

public class Upgrade2 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<FoodRecipeData> recipesToUnlock;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        foreach (var recipeData in recipesToUnlock) // Desbloquear nueva receta
        {
            RecipeProgressManager.Instance.UnlockRecipe(recipeData.FoodType);

            // Desbloquear automáticamente los ingredientes de esta receta
            foreach (var ingredientAmount in recipeData.Ingridients)
            {
                RecipeProgressManager.Instance.UnlockIngredient(ingredientAmount.IngredientType);
            }
        }

        isUnlocked =  true;
    }
}
