using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmable : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;
    public ItemType toolType = ItemType.Any;
    public int itemAmount = 1;
    public int item = -1;

    public void ApplyDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Destroy(gameObject, .5f);
        }
    }
}
