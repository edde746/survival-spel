using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmable : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;
    public ItemFlags toolType;
    public int itemAmount = 1;
    public int item = -1;

    public virtual bool ApplyDamage(float amount)
    {
        health -= amount;
        return health > 0f;
    }
}
