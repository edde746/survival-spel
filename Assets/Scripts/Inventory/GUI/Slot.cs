using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI count;

    void SetSlotActive(bool status)
    {
        icon.gameObject.SetActive(status);
        count.gameObject.SetActive(status);
    }

    public void UpdateSlot(ItemEntry entry)
    {
        if (entry.count <= 0 || entry.item == null)
        {
            SetSlotActive(false);
            return;
        }

        SetSlotActive(true);
        if (entry.item.icon == null) return;
        icon.sprite = entry.item.icon;
        if (entry.count > 1)
            count.text = entry.count.ToString();
        else
            count.text = "";
    }

    void Awake()
    {
        SetSlotActive(false);
    }
}
