using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public (Item item, int count)[,] items;
    public int hotbarActiveItem;
    // TODO: Scale for screen resolution
    Vector2 itemBoxSize;
    int itemBoxPadding = 5;
    GUIStyle selectedStyle;
    GUIStyle unselectedStyle;
    GameObject itemAnchor;
    [HideInInspector]
    public GameObject activeItemModel;
    bool showInventoryGUI = false;
    int rows, columns;
    bool dragging = false;
    (int row, int column) dragSource = (-1, -1);

    public static Inventory Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }

    void Start()
    {
        itemBoxSize = new Vector2(50f, 50f);
        items = new (Item item, int count)[4, 5];
        rows = items.GetLength(0);
        columns = items.GetLength(1);

        unselectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.linearGrayTexture } };
        selectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };
        itemAnchor = GameObject.FindGameObjectWithTag("ItemAnchor");

        // Give player test item
        GiveItem(1, 1);
        GiveItem(2, 1);
        SetActiveItem(0);
    }

    void Update()
    {
        if (Input.mouseScrollDelta.y > 0.1f) SetActiveItem(hotbarActiveItem - 1);
        else if (Input.mouseScrollDelta.y < -0.1f) SetActiveItem(hotbarActiveItem + 1);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showInventoryGUI = !showInventoryGUI;
            if (showInventoryGUI)
            {
                MouseLook.disableLook = true;
                MouseLook.lockCursor = false;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                MouseLook.disableLook = false;
                MouseLook.lockCursor = true;
            }
        }

        // Handle dragging actions
        if (showInventoryGUI)
        {
            dragging = Input.GetButton("Fire1");
        }
        else if (dragging || dragSource.row != -1)
        {
            dragging = false;
            dragSource = (-1, -1);
        }
    }

    void SetActiveItem(int hotbarIndex)
    {
        hotbarActiveItem = Mathf.Clamp(hotbarIndex, 0, 4);

        Destroy(activeItemModel);

        // Update model in hands
        var activeItem = items[0, hotbarActiveItem];
        if (activeItem.item != null && activeItem.item.model != null)
        {
            activeItemModel = Instantiate(activeItem.item.model, Vector3.zero, Quaternion.identity);
            activeItemModel.transform.SetParent(itemAnchor.transform);
            activeItemModel.transform.localEulerAngles = Vector3.zero;
            activeItemModel.transform.localPosition = activeItem.item.modelOffset;
        }
    }

    public bool GiveItem(int id, int count = 1)
    {
        if (count <= 0) return false;

        var item = ItemDatabase.GetItem(id);
        // First pass look to stack it
        if (item.stackable)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++ /* lol */)
                {
                    ref var slot = ref items[r, c];
                    if (slot.item != null && slot.item.id == id && slot.count < item.stackSize)
                    {
                        if (slot.count + count > slot.item.stackSize)
                        {
                            var itemOverflow = (slot.count + count) - slot.item.stackSize;
                            slot.count = slot.item.stackSize;
                            return GiveItem(id, itemOverflow);
                        }
                        else
                        {
                            slot.count += count;
                        }
                        return true;
                    }
                }
            }
        }

        // Put item in empty slot
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (items[r, c].count == 0)
                {
                    items[r, c] = (item, count);
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

    void DrawItemBox(Rect box, int row, int column, GUIStyle style = null)
    {
        ref var slot = ref items[row, column];
        var showAsEmpty = dragging && row == dragSource.row && column == dragSource.column;
        if (dragging)
        {
            // Get source box for drag operation if we do not have a source
            if (slot.count != 0 && box.Contains(Globals.ScreenMousePosition()) && dragSource.row == -1)
            {
                dragSource = (row, column);
            }
        }
        else if (box.Contains(Globals.ScreenMousePosition()))
        {
            // Prevent duplication 🥶
            if (dragSource.row == row && dragSource.column == column)
            {
                dragSource = (-1, -1);
            }
            else if (dragSource.row != -1)
            {
                // Handle item drop, swap slots
                ref var source = ref items[dragSource.row, dragSource.column];

                if (slot.item?.id == source.item?.id)
                {
                    Debug.Log(1);
                    var totalCount = source.count + slot.count;
                    slot.count = Mathf.Min(totalCount, slot.item.stackSize);
                    if (totalCount > slot.count)
                    {
                        source.count = totalCount - slot.count;
                    }
                }

                var copy = slot;
                slot = source;
                source = copy;
                dragSource = (-1, -1);
                SetActiveItem(hotbarActiveItem); // Ensure that we are holding the right item
            }
        }

        GUI.Box(box, slot.item != null && !showAsEmpty ? new GUIContent { image = slot.item.icon } : GUIContent.none, style != null ? style : GUIStyle.none);
        if (slot.count > 1 && !showAsEmpty) GUI.Label(box, $"{slot.count}", new GUIStyle { alignment = TextAnchor.LowerRight });
    }

    void OnGUI()
    {
        for (int i = 0; i < 5; i++)
        {
            var box = new Rect(
                (Screen.width / 2f - columns / 2f * (itemBoxSize.x + itemBoxPadding)) + i * (itemBoxSize.x + itemBoxPadding),
                Screen.height - (itemBoxSize.y + itemBoxPadding), itemBoxSize.x, itemBoxSize.y
            );

            DrawItemBox(box, 0, i, i == hotbarActiveItem ? selectedStyle : unselectedStyle);
        }

        if (showInventoryGUI)
        {
            for (int r = 1; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    var box = new Rect(
                        (Screen.width / 2f - columns / 2f * (itemBoxSize.x + itemBoxPadding)) + c * (itemBoxSize.x + itemBoxPadding),
                        Screen.height - (itemBoxSize.y + itemBoxPadding) * (r + 1.25f), itemBoxSize.x, itemBoxSize.y
                    );

                    DrawItemBox(box, r, c, unselectedStyle);
                }
            }

            if (dragging && dragSource.row != -1)
            {
                ref var dragSourceItem = ref items[dragSource.row, dragSource.column];
                GUI.Box(new Rect(Globals.ScreenMousePosition() - itemBoxSize / 2f, itemBoxSize),
                    new GUIContent { image = dragSourceItem.item.icon }, GUIStyle.none);
            }
        }
    }
}
