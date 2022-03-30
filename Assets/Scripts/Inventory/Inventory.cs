using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public (Item item, int count)[,] items;
    public int hotbarActiveItem;
    // TODO: Scale for screen resolution
    int itemBoxSize = 50;
    int itemBoxPadding = 5;
    GUIStyle selectedStyle;
    GUIStyle unselectedStyle;
    GameObject itemAnchor;

    public static Inventory Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }

    void Start()
    {
        items = new (Item item, int count)[4, 5];
        unselectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.linearGrayTexture } };
        selectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };
        itemAnchor = GameObject.FindGameObjectWithTag("ItemAnchor");

        // Give player test item
        GiveItem(1, 1);
        GiveItem(2, 1);
        // GiveItem(3, 10);
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
        var activeItem = items[0, hotbarActiveItem];
        if (activeItem.item != null && activeItem.item.model != null)
        {
            var itemModel = Instantiate(activeItem.item.model, Vector3.zero, Quaternion.identity);
            itemModel.transform.SetParent(itemAnchor.transform);
            itemModel.transform.localEulerAngles = Vector3.zero;
            itemModel.transform.localPosition = activeItem.item.modelOffset;
        }
    }

    public bool GiveItem(int id, int count = 1)
    {
        var rows = items.GetLength(0);
        var cols = items.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++ /* lol */)
            {
                if (items[r, c].count == 0)
                {
                    items[r, c] = (item: ItemDatabase.GetItem(id), count: count);
                    return true;
                }
                else if (items[r, c].item != null && items[r, c].item.id == id)
                {
                    items[r, c].count += count;
                    return true;
                }
            }
        }

        return false;
    }

    public Item GetActiveItem()
    {
        return items[0, hotbarActiveItem].item;
    }

    void OnGUI()
    {
        for (int i = 0; i < 5; i++)
        {
            var box = new Rect((Screen.width / 2 - 2.5f * (itemBoxSize + itemBoxPadding)) + i * (itemBoxSize + itemBoxPadding), Screen.height - (itemBoxSize + itemBoxPadding), itemBoxSize, itemBoxSize);
            GUI.Box(box,
                items[0, i].item != null ? new GUIContent { image = items[0, i].item.icon } : GUIContent.none,
                i == hotbarActiveItem ? selectedStyle : unselectedStyle);
            if (items[0, i].count > 0)
                GUI.Label(box, $"{items[0, i].count}", new GUIStyle { alignment = TextAnchor.LowerRight });
        }
    }
}
