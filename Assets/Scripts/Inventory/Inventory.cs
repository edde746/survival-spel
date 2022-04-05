using System;
using TMPro;
using UnityEngine;

public interface ItemHolder
{
    public Action<ItemEntry[]> OnInventoryChange { get; set; }
    public ItemEntry[] items { get; set; }
}

public class Inventory : MonoBehaviour, ItemHolder
{
    public Action<ItemEntry[]> OnInventoryChange { get; set; }
    public ItemEntry[] items { get; set; }
    public int hotbarActiveItem;
    GameObject itemAnchor;
    [HideInInspector]
    public GameObject activeItemModel;
    public static Inventory Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }

    void Start()
    {
        items = new ItemEntry[4 * 5];

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
            /*showInventoryGUI = !showInventoryGUI;
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
            }*/
        }
    }

    void SetActiveItem(int hotbarIndex)
    {
        hotbarActiveItem = Mathf.Clamp(hotbarIndex, 0, 4);

        Destroy(activeItemModel);

        // Update model in hands
        var activeItem = items[hotbarActiveItem];
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
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item?.id == item.id)
                count += items[i].count;
        }
        return count;
    }

    public void ConsumeItems(Item item, int count = 1)
    {
        var leftToConsume = count;
        for (int i = 0; i < items.Length; i++)
        {
            ref var slot = ref items[i];
            if (slot.item.id != item.id) continue;

            // Get the amount we can consume (we can't consume more than the count of the slot)
            var canConsume = Mathf.Min(leftToConsume, slot.count);

            // Consume
            slot.count -= canConsume;
            leftToConsume -= canConsume;

            // Check if we have depleated the slot
            if (slot.count == 0) slot.item = null;
            OnInventoryChange.Invoke(items);

            // We have consumed the desired amount
            if (leftToConsume == 0) return;
        }

    }

    public bool GiveItem(int id, int count = 1)
    {
        return GiveItem(ItemDatabase.GetItem(id), count);
    }

    public bool GiveItem(Item item, int count = 1)
    {
        if (count <= 0 || !item) return false;

        // First pass to stack it
        if (item.stackable > 0)
        {
            for (int i = 0; i < items.Length; i++)
            {
                ref var slot = ref items[i];
                if (slot.item != null && slot.item.id == item.id && slot.count < item.stackable)
                {
                    // Check if the new total for the slot would be more than we can stack
                    if (slot.count + count > slot.item.stackable)
                    {
                        // Get the overflow amount
                        var itemOverflow = (slot.count + count) - slot.item.stackable;
                        // Set a full stack
                        slot.count = slot.item.stackable;
                        OnInventoryChange.Invoke(items);
                        // Give the rest
                        return GiveItem(item, itemOverflow);
                    }

                    slot.count += count;
                    OnInventoryChange.Invoke(items);
                    return true;
                }
            }
        }

        // Put item in empty slot
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].count == 0)
            {
                // Check if we are giving more than stackable
                if (count > item.stackable)
                {
                    // If so, give the max count
                    items[i] = new ItemEntry(item, item.stackable);
                    OnInventoryChange.Invoke(items);
                    // Then give the rest
                    return GiveItem(item.id, count - item.stackable);
                }

                items[i] = new ItemEntry(item, count);
                OnInventoryChange.Invoke(items);
                return true;
            }
        }

        return false;
    }

    public ref ItemEntry GetActiveItem()
    {
        return ref items[hotbarActiveItem];
    }
}
