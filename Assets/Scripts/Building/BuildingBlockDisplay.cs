using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBlockDisplay : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameDisplay;
    BuildingBlock block;

    public void UpdateDisplay(BuildingBlock block)
    {
        icon.sprite = block.sprite;
        nameDisplay.text = block.name;
        this.block = block;
    }

    public void OnClick()
    {
        PlayerScript.Instance.SetActiveBuildingBlock(block);
    }
}
