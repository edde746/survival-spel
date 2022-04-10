using System.Collections.Generic;
using UnityEngine;

public class BuildingBlockDatabase : MonoBehaviour
{
    public List<BuildingBlock> blocks;

    public BuildingBlock GetBlock(int id)
    {
        foreach (BuildingBlock block in blocks)
        {
            if (block.id == id)
            {
                return block;
            }
        }
        return null;
    }

    // Singleton instance
    public static BuildingBlockDatabase Instance;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
}
