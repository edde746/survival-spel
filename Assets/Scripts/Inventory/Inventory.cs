using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Item[,] items;
    public int hotbarActiveItem;
    // TODO: Scale for screen resolution
    int itemBoxSize = 50;
    int itemBoxPadding = 5;
    GUIStyle selectedStyle;
    GUIStyle unselectedStyle;
    GameObject itemAnchor;

    void Start()
    {
        items = new Item[4, 5];
        unselectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.linearGrayTexture } };
        selectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };
        itemAnchor = GameObject.FindGameObjectWithTag("ItemAnchor");

        // Give player test item
        items[0, 0] = ItemDatabase.GetItem(1);
        items[0, 1] = ItemDatabase.GetItem(2);
    }

    void Update()
    {
        if (Input.mouseScrollDelta.y > 0.1f) SetActiveItem(hotbarActiveItem - 1);
        else if (Input.mouseScrollDelta.y < -0.1f) SetActiveItem(hotbarActiveItem + 1);
    }

    void SetActiveItem(int hotbarIndex)
    {
        hotbarActiveItem = Mathf.Clamp(hotbarIndex, 0, 4);

        // Destroy current item(s)
        foreach (Transform child in itemAnchor.transform)
        {
            Destroy(child.gameObject);
        }

        // Update model in hands
        if (items[0, hotbarActiveItem] != null)
        {
            var itemModel = Instantiate(items[0, hotbarActiveItem].model, Vector3.zero, Quaternion.identity);
            itemModel.transform.SetParent(itemAnchor.transform);
            itemModel.transform.localEulerAngles = Vector3.zero;
            itemModel.transform.localPosition = Vector3.zero;
        }
    }

    void OnGUI()
    {
        for (int i = 0; i < 5; i++)
        {
            GUI.Box(new Rect((Screen.width / 2 - 2.5f * (itemBoxSize + itemBoxPadding)) + i * (itemBoxSize + itemBoxPadding), Screen.height - (itemBoxSize + itemBoxPadding), itemBoxSize, itemBoxSize),
                items[0, i] != null ? new GUIContent { image = items[0, i].icon } : GUIContent.none,
                i == hotbarActiveItem ? selectedStyle : unselectedStyle);
        }
    }
}
