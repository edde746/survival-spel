using System;
using TMPro;
using UnityEngine;

public interface ItemHolder
{
    public ItemEntry[] items { get; set; }
}

public class Inventory : MonoBehaviour, ItemHolder
{
    public ItemEntry[] items { get; set; }
    public int hotbarActiveItem;
    GameObject itemAnchor;
    public GameObject inventoryUI;
    [HideInInspector]
    public GameObject activeItemModel;
    public AudioClip openSound;
    public static Inventory Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
        inventoryUI.SetActive(false);
    }

    void Start()
    {
        items = new ItemEntry[4 * 5];
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new ItemEntry(null, 0);
        }

        itemAnchor = GameObject.FindGameObjectWithTag("ItemAnchor");

        // Give player test item
        GiveItem(6, 1);
        //GiveItem(1, 1);
        //GiveItem(2, 1);
        GiveItem(7, 10);
        SetActiveItem(0);
    }

    KeyCode[] NumberKeys = {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
    };

    void Update()
    {
        // Hotbar scrolling
        if (Input.mouseScrollDelta.y > 0.1f) SetActiveItem(hotbarActiveItem - 1);
        else if (Input.mouseScrollDelta.y < -0.1f) SetActiveItem(hotbarActiveItem + 1);

        // Handle opening & closing of inventory
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            AudioSource.PlayClipAtPoint(openSound, transform.position);

            Globals.SetGUICursorActive(inventoryUI.activeSelf);
        }

        for (int i = 0; i < NumberKeys.Length; i++)
        {
            if (Input.GetKeyDown(NumberKeys[i]))
            {
                // Consume item if it's food
                if (items[i].item != null && items[i].item.flags.HasFlag(ItemFlags.Food))
                {
                    // Don't eat when full
                    if (PlayerScript.Instance.hunger < 98f)
                    {
                        var targetHunger = Mathf.Clamp(PlayerScript.Instance.hunger + items[i].item.GetStat("hunger"), 0f, 100f);
                        PlayerScript.Instance.hunger = targetHunger;
                        items[i].Consume(1);
                        // Play sound
                        Globals.PlayEatSound(transform.position);
                    }
                }
                else
                {
                    SetActiveItem(i);
                }
            }
        }
    }

    public void SetActiveItem(int hotbarIndex = -1)
    {
        if (hotbarIndex != -1)
            hotbarActiveItem = Mathf.Clamp(hotbarIndex, 0, 4);

        Destroy(activeItemModel);

        // Update model in hands
        var activeItem = items[hotbarActiveItem];
        if (activeItem?.item != null && activeItem.item.model != null && !activeItem.item.flags.HasFlag(ItemFlags.DontShowInHand))
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
            if (slot == null || slot.item?.id != item.id) continue;

            // Get the amount we can consume (we can't consume more than the count of the slot)
            var canConsume = Mathf.Min(leftToConsume, slot.count);

            // Consume
            slot.count -= canConsume;
            leftToConsume -= canConsume;

            // Check if we have depleated the slot
            if (slot.count == 0) slot.item = null;
            slot.OnChange?.Invoke();

            // We have consumed the desired amount
            if (leftToConsume == 0) return;
        }
    }

    public bool GiveItem(int id, int count = 1)
    {
        return GiveItem(ItemDatabase.GetItem(id), count);
    }

    // TODO: Inventory overflow
    public bool GiveItem(Item item, int count = 1)
    {
        if (count <= 0 || !item) return false;

        // First pass to stack it
        if (item.stackable > 0)
        {
            for (int i = 0; i < items.Length; i++)
            {
                ref var slot = ref items[i];
                if (slot != null && slot.item != null && slot.item.id == item.id && slot.count < item.stackable)
                {
                    // Check if the new total for the slot would be more than we can stack
                    var newCount = slot.count + count;
                    if (newCount > slot.item.stackable)
                    {
                        // Get the overflow amount
                        var itemOverflow = newCount - slot.item.stackable;
                        // Set a full stack
                        slot.count = slot.item.stackable;
                        slot.OnChange?.Invoke();
                        // Give the overflow amount
                        if (itemOverflow > 0)
                            return GiveItem(item, itemOverflow);
                        return true;
                    }

                    slot.count += count;
                    slot.OnChange?.Invoke();
                    return true;
                }
            }
        }

        // Put item in empty slot
        for (int i = 0; i < items.Length; i++)
        {
            var slot = items[i];
            if (slot.count == 0)
            {
                // Check if we are giving more than stackable
                if (count > item.stackable)
                {
                    // If so, give the max count
                    slot.item = item;
                    slot.count = item.stackable;
                    var overflow = count - item.stackable;
                    // Then give the rest
                    if (overflow > 0)
                        return GiveItem(item, overflow);
                    slot.OnChange?.Invoke();
                    return true;
                }

                slot.item = item;
                slot.count = count;
                slot.OnChange?.Invoke();
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
