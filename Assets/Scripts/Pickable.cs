using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    public int item = -1;
    public int itemAmount = 1;
    public List<AudioClip> pickSounds;

    public void Pick()
    {
        Inventory.Instance.GiveItem(item, itemAmount);
        // Play random pick sound
        if (pickSounds.Count > 0)
        {
            var clip = pickSounds[Random.Range(0, pickSounds.Count)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        Destroy(gameObject);
    }
}
