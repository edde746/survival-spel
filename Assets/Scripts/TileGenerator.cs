using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject tile;
    int width = (int)Mathf.Ceil(WorldGeneration.size / 250f);
    void Awake()
    {
        var generator = tile.GetComponent<TerrainGenerator>();
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Instantiate(tile, new Vector3(generator.size * x, 0f, generator.size * z), Quaternion.identity);
            }
        }
    }
}
