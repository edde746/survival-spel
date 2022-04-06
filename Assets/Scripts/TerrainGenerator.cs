using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;
    public int size = 250;
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
        vertecies = new Vector3[(size + 1) * (size + 1)];

        for (int idx = 0, z = 0; z <= size; z++)
        {
            for (int x = 0; x <= size; x++, idx++)
            {
                float value = 0f, normal = 0f;
                float worldX = (x + transform.position.x);
                float worldZ = (z + transform.position.z);
                int layerIdx = 0;
                foreach (var layer in WorldGeneration.Instance.layers)
                {
                    if (!layerSeed.ContainsKey(layerIdx))
                        layerSeed.Add(layerIdx, Mathf.Ceil(Random.Range(1000f, 9999f)));
                    value += layer.amplitude * Mathf.PerlinNoise(worldX / WorldGeneration.size * layer.frequency, worldZ / WorldGeneration.size * layer.frequency);
                    normal += layer.amplitude;
                    layerIdx++;
                }

                // Falloff
                value *= 1f - Mathf.Max(Mathf.Abs(worldX / WorldGeneration.size * 2 - 1), Mathf.Abs(worldZ / WorldGeneration.size * 2 - 1));

                vertecies[idx] = new Vector3(x, Mathf.Clamp01(value / normal) * WorldGeneration.Instance.peaks, z);
            }
        }

        int vertex = 0, triangle = 0;
        triangles = new int[size * size * 6];

        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                triangles[triangle + 0] = vertex + 0;
                triangles[triangle + 1] = vertex + size + 1;
                triangles[triangle + 2] = vertex + 1;
                triangles[triangle + 3] = vertex + 1;
                triangles[triangle + 4] = vertex + size + 1;
                triangles[triangle + 5] = vertex + size + 2;

                vertex++;
                triangle += 6;
            }

            vertex++;
        }

        colors = new Color[vertecies.Length];

        for (int idx = 0, z = 0; z <= size; z++)
        {
            for (int x = 0; x <= size; x++, idx++)
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
