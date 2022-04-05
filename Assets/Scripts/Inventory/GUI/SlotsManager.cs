using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlotsManager : MonoBehaviour
{
    public GameObject itemHolder;
    public GameObject slotPrefab;
    public int limit = 0, offset = 0;

    void Draw(ItemEntry[] data)
    {
        if (!gameObject.activeSelf) return;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var dataIter = data.Skip(offset);
        if (limit > 0)
            dataIter = dataIter.Take(limit);

        foreach (var entry in dataIter)
        {
            var newSlot = Instantiate(slotPrefab, transform, false);
            var slotScript = newSlot.GetComponent<Slot>();
            slotScript.UpdateSlot(entry);
        }
    }

    void Awake()
    {
        itemHolder.GetComponent<ItemHolder>().OnInventoryChange += Draw;
    }
}
