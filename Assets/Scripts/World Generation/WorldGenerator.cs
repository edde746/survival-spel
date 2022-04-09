using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int size = 600;
    public int tileSize = 100;
    public float heightScale = 100f;
    public int seed;
    public float scale = 1f;

    public int octaves = 4;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2f;
    public Vector2 offset;
    public AnimationCurve falloffCurve;
    public GameObject tilePrefab;
    float[] map;

    Texture2D texture;

    public float[] GenerateFalloffMap(int size)
    {
        var map = new float[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xCoord = (float)x / size * 2 - 1;
                float yCoord = (float)y / size * 2 - 1;

                map[y * size + x] = falloffCurve.Evaluate(1f - Mathf.Max(Mathf.Abs(xCoord), Mathf.Abs(yCoord)));
            }
        }

        return map;
    }

    void Awake()
    {
        // Generate maps
        var falloffMap = GenerateFalloffMap(size);
        map = Noise.GenerateNoiseMap(size, seed, scale, octaves, persistance, lacunarity, offset);
        GetComponent<Erosion>().Erode(map, size, 90000);

        Debug.Log("Generated maps");

        // Instantiate tiles and apply meshes
        for (int y = 0; y < size / tileSize; y++)
        {
            for (int x = 0; x < size / tileSize; x++)
            {
                // Get tile
                var tile = Instantiate(tilePrefab, new Vector3(x * tileSize * tilePrefab.transform.localScale.x, transform.position.y, y * tileSize * tilePrefab.transform.localScale.z), Quaternion.identity);
                tile.transform.parent = transform;

                // Get mesh
                var mesh = tile.GetComponent<MeshFilter>().mesh;

                // Generate mesh
                var vertices = new Vector3[(tileSize + 1) * (tileSize + 1)];

                // Apply heightmap
                for (int i = 0, yy = 0; yy <= tileSize; yy++)
                {
                    for (int xx = 0; xx <= tileSize; xx++)
                    {
                        // Get index of map
                        int index = (y * tileSize + yy) * (size) + (x * tileSize + xx);
                        vertices[i] = new Vector3(xx, map[index] * heightScale * falloffMap[index], yy);
                        i++;
                    }
                }

                // Generate Triangles
                var triangles = new int[tileSize * tileSize * 6];
                for (int vertex = 0, triangle = 0, z = 0; z < tileSize; z++)
                {
                    for (int j = 0; j < tileSize; j++)
                    {
                        triangles[triangle + 0] = vertex + 0;
                        triangles[triangle + 1] = vertex + tileSize + 1;
                        triangles[triangle + 2] = vertex + 1;
                        triangles[triangle + 3] = vertex + 1;
                        triangles[triangle + 4] = vertex + tileSize + 1;
                        triangles[triangle + 5] = vertex + tileSize + 2;

                        vertex++;
                        triangle += 6;
                    }

                    vertex++;
                }

                // Apply mesh
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                // Apply mesh collider
                var meshCollider = tile.GetComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
        }

        Debug.Log("Generated tiles");
    }
}
