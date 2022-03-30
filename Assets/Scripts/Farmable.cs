using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmable : MonoBehaviour
{
    public float health = 100f;
    public ItemType toolType = ItemType.Any;
    public int itemAmount = 1;
    public int item = -1;
}
