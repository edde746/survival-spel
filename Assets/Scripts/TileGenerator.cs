using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject tile;
    public int width, height;
    void Awake()
    {
        var generator = tile.GetComponent<TerrainGenerator>();
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < height; x++)
            {
                Instantiate(tile, new Vector3(generator.width * x, 0f, generator.height * z), Quaternion.identity);
            }
        }
    }
}
