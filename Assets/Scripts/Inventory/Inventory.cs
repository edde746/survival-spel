using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public ItemEntry[,] items;
    public int hotbarActiveItem;
    // TODO: Scale for screen resolution
    [HideInInspector]
    public Vector2 itemBoxSize;
    [HideInInspector]
    public int itemBoxPadding = 5;
    GUIStyle selectedStyle;
    GUIStyle unselectedStyle;
    GameObject itemAnchor;
    [HideInInspector]
    public GameObject activeItemModel;
    public bool showInventoryGUI = false;
    [HideInInspector]
    public int rows, columns;
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
        items = new ItemEntry[4, 5];
        rows = items.GetLength(0);
        columns = items.GetLength(1);

        unselectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.linearGrayTexture } };
        selectedStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };
        itemAnchor = GameObject.FindGameObjectWithTag("ItemAnchor");

        // Give player test item
        GiveItem(6, 1);
        //GiveItem(1, 1);
        //GiveItem(2, 1);
        //GiveItem(5, 2);
        SetActiveItem(0);
    }

    void Update()
    {
        // Hotbar scrolling
        if (Input.mouseScrollDelta.y > 0.1f) SetActiveItem(hotbarActiveItem - 1);
        else if (Input.mouseScrollDelta.y < -0.1f) SetActiveItem(hotbarActiveItem + 1);

        // Handle opening & closing of inventory
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
                Crafting.Instance.showCraftingGUI = false;
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
        if (activeItem.item != null && activeItem.item.model != null && !activeItem.item.flags.HasFlag(ItemFlags.DontShowInHand))
        {
            activeItemModel = Instantiate(activeItem.item.model, Vector3.zero, Quaternion.identity);
            activeItemModel.transform.SetParent(itemAnchor.transform);
            activeItemModel.transform.localEulerAngles = Vector3.zero;
            activeItemModel.transform.localPosition = Vector3.zero;
            return;
        }

        activeItemModel = null;
    }

    public int TallyItems(Item item)
    {
        int count = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (items[r, c].item?.id == item.id)
                    count += items[r, c].count;
            }
        }
        return count;
    }

    public void ConsumeItems(Item item, int count = 1)
    {
        var leftToConsume = count;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                ref var slot = ref items[r, c];
                if (slot.item.id != item.id) continue;

                // Get the amount we can consume (we can't consume more than the count of the slot)
                var canConsume = Mathf.Min(leftToConsume, slot.count);

                // Consume
                slot.count -= canConsume;
                leftToConsume -= canConsume;

                // Check if we have depleated the slot
                if (slot.count == 0) slot.item = null;

                // We have consumed the desired amount
                if (leftToConsume == 0) return;
            }
        }
    }

    public bool GiveItem(int id, int count = 1)
    {
        return GiveItem(ItemDatabase.GetItem(id), count);
    }

    // 🍝🍝🍝
    public bool GiveItem(Item item, int count = 1)
    {
        if (count <= 0 || !item) return false;

        // First pass to stack it
        if (item.stackable > 0)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++ /* lol */)
                {
                    ref var slot = ref items[r, c];
                    if (slot.item != null && slot.item.id == item.id && slot.count < item.stackable)
                    {
                        // Check if the new total for the slot would be more than we can stack
                        if (slot.count + count > slot.item.stackable)
                        {
                            // Get the overflow amount
                            var itemOverflow = (slot.count + count) - slot.item.stackable;
                            // Set a full stack
                            slot.count = slot.item.stackable;
                            // Give the rest
                            return GiveItem(item, itemOverflow);
                        }

                        slot.count += count;
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
                    // Check if we are giving more than stackable
                    if (count > item.stackable)
                    {
                        // If so, give the max count
                        items[r, c] = new ItemEntry(item, item.stackable);
                        // Then give the rest
                        return GiveItem(item.id, count - item.stackable);
                    }

                    items[r, c] = new ItemEntry(item, count);
                    return true;
                }
            }
        }

        return false;
    }

    public ref ItemEntry GetActiveItem()
    {
        return ref items[0, hotbarActiveItem];
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
                    var totalCount = source.count + slot.count;
                    slot.count = Mathf.Min(totalCount, slot.item.stackable);
                    if (totalCount > slot.count)
                    {
                        source.count = totalCount - slot.count;
                    }
                }

                // Swap the slots
                var copy = slot;
                slot = source;
                source = copy;

                // Release the source, refresh the active item incase it was switched
                dragSource = (-1, -1);
                SetActiveItem(hotbarActiveItem);
            }
        }

        // Draw item box & count
        GUI.Box(box, slot.item != null && !showAsEmpty ? new GUIContent { image = slot.item.icon } : GUIContent.none, style != null ? style : GUIStyle.none);
        if (slot.count > 1 && !showAsEmpty) GUI.Label(box, $"{slot.count}", new GUIStyle { alignment = TextAnchor.LowerRight });
    }

    void OnGUI()
    {
        // Hide the inventory UI when the crafting UI is active
        if (Crafting.Instance.showCraftingGUI) return;

        // Draw the hotbar
        for (int i = 0; i < 5; i++)
        {
            var box = new Rect(
                (Screen.width / 2f - columns / 2f * (itemBoxSize.x + itemBoxPadding)) + i * (itemBoxSize.x + itemBoxPadding),
                Screen.height - (itemBoxSize.y + itemBoxPadding), itemBoxSize.x, itemBoxSize.y
            );

            DrawItemBox(box, 0, i, i == hotbarActiveItem ? selectedStyle : unselectedStyle);
        }

        // Early return if UI is closed
        if (!showInventoryGUI) return;

        // Draw rest of inventory (all except hotbar that is already drawn)
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

        // Draw the item we are currently dragging if we are currently dragging
        if (dragging && dragSource.row != -1)
        {
            ref var dragSourceItem = ref items[dragSource.row, dragSource.column];
            GUI.Box(new Rect(Globals.ScreenMousePosition() - itemBoxSize / 2f, itemBoxSize),
                new GUIContent { image = dragSourceItem.item.icon }, GUIStyle.none);
        }

        // Button to go to crafting UI
        if (GUI.Button(new Rect((
            Screen.width / 2f - (columns / 2f) * (itemBoxSize.x + itemBoxPadding)), Screen.height - (itemBoxSize.y + itemBoxPadding) * (rows + 1.25f) + 25f,
            (itemBoxSize.x + itemBoxPadding) * 5, 25f), "Crafting"
        )) Crafting.Instance.showCraftingGUI = true;
    }
}
