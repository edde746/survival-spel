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
    public List<AudioClip> hitSounds;
    public List<AudioClip> dieSounds;

    public virtual bool ApplyDamage(float amount)
    {
        health -= amount;
        // Play random hit sound
        if (hitSounds.Count > 0)
        {
            var clip = hitSounds[Random.Range(0, hitSounds.Count)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        // Play death sound if health is 0
        if (health <= 0f && dieSounds.Count > 0)
        {
            var clip = dieSounds[Random.Range(0, dieSounds.Count)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        return health > 0f;
    }
}
