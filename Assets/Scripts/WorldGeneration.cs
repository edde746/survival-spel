using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [System.Serializable]
    public struct Layer
    {
        public float frequency;
        public float amplitude;
    }

    public List<Layer> layers;
    public static float size = 1000f;
    public float peaks = 100f;
    public AnimationCurve heightCurve;

    public static Vector3 centerPoint;
    public Gradient heightColors;
    public int seed;

    public static WorldGeneration Instance { get; private set; }

    void Awake()
    {
        Random.InitState(seed);
        if (Instance != null)
            return;

        Instance = this;
        centerPoint = new Vector3(size / 2f, 0f, size / 2f);
    }
}
