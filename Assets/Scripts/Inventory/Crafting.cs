using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour
{
    public List<Recipe> recipes;
    Vector2 scrollPosition;
    [HideInInspector]
    public bool showCraftingGUI = false;
    public static Crafting Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }

    void OnGUI()
    {
        if (!showCraftingGUI) return;
        var boxSize = Inventory.Instance.itemBoxSize + new Vector2(Inventory.Instance.itemBoxPadding, Inventory.Instance.itemBoxPadding);
        var guiWidth = boxSize.x * Inventory.Instance.columns;

        if (GUI.Button(new Rect((
            Screen.width / 2f - (Inventory.Instance.columns / 2f) * boxSize.x), Screen.height - boxSize.y * (Inventory.Instance.rows + 1.25f) + 25f,
            guiWidth, 25f), "Back"
        )) Crafting.Instance.showCraftingGUI = false;

        GUILayout.BeginArea(new Rect(
            (Screen.width / 2f - (Inventory.Instance.columns / 2f) * boxSize.x), Screen.height - boxSize.y * Inventory.Instance.rows - 15f,
            guiWidth, Inventory.Instance.rows * boxSize.y));

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(guiWidth), GUILayout.Height(Inventory.Instance.rows * boxSize.y));

        foreach (var recipe in recipes)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button(new GUIContent { image = recipe.output.icon }, GUILayout.Width(50f), GUILayout.Height(50f));
            var requirements = "";
            foreach (var input in recipe.input)
                requirements += $"{input.count}x {input.item.name}\n";

            GUILayout.Label($"Requires:\n{requirements}");
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

    }
}
