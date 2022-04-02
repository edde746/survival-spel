using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;
    public int width = 250;
    public int height = 250;
    Vector3[] vertecies;
    int[] triangles;
    Color[] colors;
    static Dictionary<int, float> layerSeed;

    void Awake()
    {
        if (layerSeed == null)
            layerSeed = new Dictionary<int, float>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateGrid();
        UpdateMesh();
    }

    void CreateGrid()
    {
        vertecies = new Vector3[(width + 1) * (height + 1)];

        for (int idx = 0, z = 0; z <= width; z++)
        {
            for (int x = 0; x <= height; x++, idx++)
            {
                float value = 0f, normal = 0f;
                float worldX = (x + transform.position.x);
                float worldZ = (z + transform.position.z);
                int layerIdx = 0;
                foreach (var layer in WorldGeneration.Instance.layers)
                {
                    if (!layerSeed.ContainsKey(layerIdx))
                        layerSeed.Add(layerIdx, Mathf.Floor(Random.Range(1000f, 9999f)));
                    value += layer.amplitude * Mathf.PerlinNoise(worldX * layer.frequency + layerSeed[layerIdx], worldZ * layer.frequency + layerSeed[layerIdx]);
                    normal += layer.amplitude;
                    layerIdx++;
                }
                value = value * (1f - Mathf.Clamp01(Vector3.Distance(WorldGeneration.centerPoint, new Vector3(worldX, 0f, worldZ)) / 500f));

                vertecies[idx] = new Vector3(x, Mathf.Clamp01(value / normal) * WorldGeneration.Instance.peaks, z);
            }
        }

        int vertex = 0, triangle = 0;
        triangles = new int[width * height * 6];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[triangle + 0] = vertex + 0;
                triangles[triangle + 1] = vertex + width + 1;
                triangles[triangle + 2] = vertex + 1;
                triangles[triangle + 3] = vertex + 1;
                triangles[triangle + 4] = vertex + width + 1;
                triangles[triangle + 5] = vertex + width + 2;

                vertex++;
                triangle += 6;
            }

            vertex++;
        }

        colors = new Color[vertecies.Length];

        for (int idx = 0, z = 0; z <= width; z++)
        {
            for (int x = 0; x <= height; x++, idx++)
            {
                colors[idx] = WorldGeneration.Instance.heightColors.Evaluate(vertecies[idx].y / WorldGeneration.Instance.peaks);
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertecies;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
