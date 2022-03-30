using UnityEngine;
using System.Collections.Generic;

public enum ItemType
{
    Any = 0,
    Axe = 1,
    Pickaxe = 2,
    Resource = 3
}

[System.Serializable]
public class Stat
{
    public string name;
    public float value;
}

[System.Serializable]
public class Item
{
    public int id = -1;
    public string name;
    public ItemType type;
    public bool stackable = true;
    public Texture icon;
    public GameObject model;
    public Vector3 modelOffset = Vector3.zero;
    [SerializeField]
    public List<Stat> stats;

    public float GetStat(string name)
    {
        var stat = stats.Find(stat => stat.name == name);
        return stat != null ? stat.value : 0f;
    }
}
