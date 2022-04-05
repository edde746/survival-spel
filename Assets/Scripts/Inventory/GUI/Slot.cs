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
        if (entry.count <= 0 || entry.item == null)
        {
            SetSlotActive(false);
            return;
        }

        SetSlotActive(true);
        count.text = "";
        if (entry.item.icon == null) return;
        icon.sprite = entry.item.icon;
        if (entry.count > 1)
            count.text = entry.count.ToString();
    }

    void Awake()
    {
        SetSlotActive(false);
    }

    public void OnDrop(PointerEventData data)
    {
        if (data.pointerDrag != null && data.pointerDrag != gameObject)
        {
            var sourceSlot = data.pointerDrag.GetComponent<Slot>();
            if (!sourceSlot) return;

            // Swap contents of item entries

            if (sourceSlot.entry.item?.id == entry.item?.id)
            {
                if (entry.item.stackable > 0)
                {
                    var total = entry.count + sourceSlot.entry.count;
                    if (total > entry.item.stackable)
                    {
                        entry.count = entry.item.stackable;
                        sourceSlot.entry.count = total - entry.item.stackable;
                    }
                    else
                    {
                        entry.count = total;
                    }
                }
            }

            var itemCopy = entry.item;
            var countCopy = entry.count;

            entry.item = sourceSlot.entry.item;
            entry.count = sourceSlot.entry.count;

            sourceSlot.entry.item = itemCopy;
            sourceSlot.entry.count = countCopy;

            // Visual update
            UpdateSlot(entry);
            Inventory.Instance.SetActiveItem();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        icon.transform.position = Input.mousePosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        icon.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        icon.transform.localScale = Vector3.one * 1.1f;
        icon.transform.SetAsLastSibling();
        count.gameObject.SetActive(false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Reset item
        icon.transform.SetParent(transform);
        icon.transform.localPosition = Vector3.zero;
        icon.transform.localScale = Vector3.one;
        UpdateSlot(entry);
    }
}
