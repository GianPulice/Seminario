using System.Collections.Generic;
using UnityEngine;

public class Upgrade5 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<FoodRecipeData> recipesToUnlock;
    [SerializeField] private SliderCleanDiirtyTableUIData sliderCleanDiirtyTableUIData;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    // Provisorio restaurar el valor del MaxHoldTime porque los scriptable objects no restauran su valor
    void OnApplicationQuit()
    {
        sliderCleanDiirtyTableUIData.MaxHoldTime = 5f;
    }


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

        sliderCleanDiirtyTableUIData.MaxHoldTime *= 0.65f; // Aumentar tiempo de limpieza de las mesas.

        isUnlocked = true;
    }
}
