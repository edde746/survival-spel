using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlotsManager : MonoBehaviour
{
    public GameObject itemHolder;
    public GameObject slotPrefab;
    public int limit = 0, offset = 0;

    void UpdateSlots()
    {
        if (!gameObject.activeSelf) return;
    }

    void Start()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var index = 0;
        var items = itemHolder.GetComponent<ItemHolder>().items;
        foreach (var entry in items)
        {
            if (limit > 0 && index > offset + limit) return;
            if (offset > 0 && index < offset)
            {
                index++;
                continue;
            }

            var newSlot = Instantiate(slotPrefab, transform, false);
            var slotScript = newSlot.GetComponent<Slot>();
            slotScript.UpdateSlot(entry);

            index++;
        }
    }
}
