using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeButton : GenericTweenButton
{
    [Header("RecipeInfo")]
    [SerializeField] private FoodType recipeType;
    public FoodType RecipeType => recipeType;
}
