using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    public int item = -1;
    public int itemAmount = 1;

    public void Pick()
    {
        Inventory.Instance.GiveItem(item, itemAmount);
        Destroy(gameObject);
    }
}
