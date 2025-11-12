using System.Collections.Generic;
using UnityEngine;

public class Upgrade1 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<GameObject> debris;
    [SerializeField] private List<Table> tablesToActive;
    [SerializeField] private List<FoodRecipeData> recipesToUnlock;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        if (debris != null) // Desbloquear escombros
        {
            foreach (var debris in debris) 
            {
                debris.gameObject.SetActive(false);
            }
        }

        if (tablesToActive != null)
        {
            foreach (var table in tablesToActive) // Desbloquear mesas
            {
                table.gameObject.SetActive(true);
            }
        }

        ClientManager.Instance.ClientManagerData.TimeToWaitForSpawnNewClient = 3; // Aumentar el tiempo
        ClientManager.Instance.AvailableClientTypes.Add(ClientType.Ogre); // Agregar nuevo cliente

        foreach (var recipeData in recipesToUnlock) // Desbloquear nueva receta
        {
            RecipeProgressManager.Instance.UnlockRecipe(recipeData.FoodType);

            // Desbloquear automáticamente los ingredientes de esta receta
            foreach (var ingredientAmount in recipeData.Ingridients)
            {
                RecipeProgressManager.Instance.UnlockIngredient(ingredientAmount.IngredientType);
            }
        }

        isUnlocked = true;
    }
}
