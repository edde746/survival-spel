using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Image icon;
    public TextMeshProUGUI count;
    [HideInInspector]
    ItemEntry entry; // Hopefully this is a reference?

    void SetSlotActive(bool status)
    {
        icon.gameObject.SetActive(status);
        count.gameObject.SetActive(status);
    }

    public void UpdateSlot(ItemEntry entry)
    {
        this.entry = entry;
        if (entry == null || entry.count <= 0 || entry.item == null)
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

    public void OnDrop(PointerEventData data)
    {
        if (data.pointerDrag != null)
        {
            var sourceSlot = data.pointerDrag.GetComponent<Slot>();
            if (!sourceSlot) return;

            // Swap contents of item entries
            // TODO: Stacking
            var itemCopy = entry.item;
            var countCopy = entry.count;

            entry.item = sourceSlot.entry.item;
            entry.count = sourceSlot.entry.count;

            sourceSlot.entry.item = itemCopy;
            sourceSlot.entry.count = countCopy;

            // Visual update
            UpdateSlot(entry);
            sourceSlot.UpdateSlot(sourceSlot.entry);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        icon.transform.position = Input.mousePosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        icon.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        icon.transform.SetAsLastSibling();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Reset item
        icon.transform.SetParent(transform);
        icon.transform.localPosition = Vector3.zero;
    }
}
