using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Farmable
{
    Tree()
    {
        toolType = ItemFlags.MineWood;
        item = 4;
    }

    public override bool ApplyDamage(float amount)
    {
        if (!base.ApplyDamage(amount))
        {
            Destroy(gameObject, .5f);
            return false;
        }

        return true;
    }
}
