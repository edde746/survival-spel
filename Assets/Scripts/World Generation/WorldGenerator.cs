using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int width = 400, height = 600;
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

    public float[,] GenerateFalloffMap(int width, int height)
    {
        var map = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xCoord = (float)x / width * 2 - 1;
                float yCoord = (float)y / height * 2 - 1;

                map[x, y] = falloffCurve.Evaluate(1f - Mathf.Max(Mathf.Abs(xCoord), Mathf.Abs(yCoord)));
            }
        }

        return map;
    }

    void Awake()
    {
        // Generate maps
        var falloffMap = GenerateFalloffMap(width + 1, height + 1);
        var noiseMap = Noise.GenerateNoiseMap(width + 1, height + 1, seed, scale, octaves, persistance, lacunarity, offset);

        // Instantiate tiles and apply meshes
        for (int y = 0; y < height; y += tileSize)
        {
            for (int x = 0; x < width; x += tileSize)
            {
                // Get tile
                var tile = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                tile.transform.parent = transform;

                // Get mesh
                var mesh = tile.GetComponent<MeshFilter>().mesh;

                // Generate mesh
                var vertices = new Vector3[(tileSize + 1) * (tileSize + 1)];

                // Apply heightmap
                for (int i = 0, z = 0; z <= tileSize; z++)
                {
                    for (int j = 0; j <= tileSize; j++)
                    {
                        var height = noiseMap[x + j, y + z] * heightScale * falloffMap[x + j, y + z];
                        vertices[i] = new Vector3(j, height, z);
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
    }

}
