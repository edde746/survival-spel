using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Farmable
{
    Rock()
    {
        toolType = ItemType.Pickaxe;
        item = 3;
    }

    public override bool ApplyDamage(float amount)
    {
        if (!base.ApplyDamage(amount))
        {
            StartCoroutine(DeathParticle());
            return false;
        }

        return true;
    }

    IEnumerator DeathParticle()
    {
        yield return new WaitForSeconds(.5f);
        Instantiate(Globals.Instance.rockDestroyParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
