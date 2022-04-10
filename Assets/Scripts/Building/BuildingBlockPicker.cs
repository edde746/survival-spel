using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildingBlockPicker : MonoBehaviour
{
    public GameObject blockPrefab;
    void Awake()
    {
        // Destroy children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Set up blocks
        foreach (BuildingBlock block in BuildingBlockDatabase.Instance.blocks)
        {
            var blockObject = Instantiate(blockPrefab, transform, false);
            blockObject.GetComponent<BuildingBlockDisplay>().UpdateDisplay(block);
        }
    }
}
