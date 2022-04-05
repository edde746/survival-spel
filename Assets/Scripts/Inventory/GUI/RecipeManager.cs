using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public GameObject recipeSlotPrefab;
    void Start()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var recipe in Crafting.Instance.recipes)
        {
            var newSlot = Instantiate(recipeSlotPrefab, transform, false);
            newSlot.GetComponent<RecipeSlot>().SetRecipe(recipe);
        }
    }
}
