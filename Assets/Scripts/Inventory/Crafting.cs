using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour
{
    public List<Recipe> recipes;
    public static Crafting Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }

    public void Craft(Recipe recipe)
    {
        // Check that all requirements are met
        foreach (var requirement in recipe.input)
        {
            if (Inventory.Instance.TallyItems(requirement.item) < requirement.count)
                return;
        }

        // Only consume the items once we know that we have all the required items
        foreach (var requirement in recipe.input)
        {
            Inventory.Instance.ConsumeItems(requirement.item, requirement.count);
        }

        Inventory.Instance.GiveItem(recipe.output, 1);
        Globals.CreateNotification("Crafted " + recipe.output.name, 5f, recipe.output.icon);
        Globals.PlayCraftSound(PlayerScript.Instance.transform.position);
    }
}
