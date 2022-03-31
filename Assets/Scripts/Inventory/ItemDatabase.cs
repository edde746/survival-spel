using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public List<Item> items;
    public List<Recipe> recipes;

    public static Item GetItem(int id)
    {
        return Instance.items.Find(item => item.id == id);
    }

    public static ItemDatabase Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }
}
