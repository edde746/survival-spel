using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Layer
{
    public float seed;
    public float frequency;
    public float amplitude;
}

public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;
    public int width = 0;
    public int height = 0;
    public float peaks = 100f;
    Vector3[] vertecies;
    int[] triangles;
    public List<Layer> layers;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateGrid();
        UpdateMesh();
    }

    void CreateGrid()
    {
        vertecies = new Vector3[(width + 1) * (height + 1)];

        var idx = 0;
        for (int z = 0; z <= width; z++)
        {
            for (int x = 0; x <= height; x++)
            {
                float value = 0f, normal = 0f;
                foreach (var layer in layers)
                {
                    value += layer.amplitude * Mathf.PerlinNoise((x + transform.position.x) * layer.frequency + layer.seed, (z + transform.position.z) * layer.frequency + layer.seed);
                    normal += layer.amplitude;
                }
                vertecies[idx] = new Vector3(x, Mathf.Clamp01(value / normal) * peaks, z);
                idx++;
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

    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertecies;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
