using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public float money;

    public string lastSceneName;

    public List<IngredientStock> ingredientInventory = new List<IngredientStock>();

}


[Serializable]
public class IngredientStock
{
    public IngredientType ingredientType;
    public int amount;
}