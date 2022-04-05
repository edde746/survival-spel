using System;
using UnityEngine;
using System.Collections.Generic;

[System.Flags]
public enum ItemFlags
{
    MineWood = 1 << 1,
    MineRock = 1 << 2,
    Resource = 1 << 3,
    DontShowInHand = 1 << 4,
    Placeable = 1 << 5,
    Tool = 1 << 6,
    Food = 1 << 7
}

[System.Serializable]
public class Stat
{
    public string name;
    public float value;
}

[CreateAssetMenu(fileName = "New Item", menuName = "Assets/Item")]
public class Item : ScriptableObject
{
    public int id = -1;
    public ItemFlags flags;
    public int stackable = 1;
    public Sprite icon;
    public GameObject model;
    [SerializeField]
    public List<Stat> stats;

    public float GetStat(string name)
    {
        var stat = stats.Find(stat => stat.name == name);
        return stat != null ? stat.value : 1f;
    }
}

[System.Serializable]
public class ItemEntry
{
    public ItemEntry(Item item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public Action OnChange;
    public Item item;
    public int count;

    public void Consume(int amount)
    {
        count -= amount;
        if (count <= 0)
        {
            item = null;
            count = 0;
        }

        OnChange?.Invoke();
    }
}