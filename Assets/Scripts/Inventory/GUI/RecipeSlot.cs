using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour
{
    public Image icon;
    [HideInInspector]
    public Recipe recipe;

    public void SetRecipe(Recipe recipe)
    {
        this.recipe = recipe;
        icon.sprite = recipe.output.icon;
    }

    public void OnClick()
    {
        SelectedRecipeLoader.Instance.LoadRecipe(recipe);
    }
}
