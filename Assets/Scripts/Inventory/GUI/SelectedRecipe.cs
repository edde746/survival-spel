using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedRecipe : MonoBehaviour
{
    public GameObject ingredientPrefab;
    public Transform ingredientContainer;
    public Image icon;
    public TextMeshProUGUI nameText;
    [HideInInspector]
    public Recipe recipe;

    public static SelectedRecipe Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
        LoadRecipe(Crafting.Instance.recipes[0]);
    }

    public void LoadRecipe(Recipe recipe)
    {
        this.recipe = recipe;
        icon.sprite = recipe.output.icon;
        nameText.text = recipe.output.name;
        foreach (Transform child in ingredientContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var input in recipe.input)
        {
            var ingredient = Instantiate(ingredientPrefab, ingredientContainer, false);
            ingredient.GetComponent<Ingredient>().SetIngredient(input.item, input.count);
        }
    }

    public void Craft()
    {
        Crafting.Instance.Craft(recipe);
    }
}
