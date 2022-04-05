using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Ingredient : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI count;
    public void SetIngredient(Item item, int count)
    {
        if (icon && item != null)
            icon.sprite = item.icon;
        this.count.text = count.ToString();
    }
}
