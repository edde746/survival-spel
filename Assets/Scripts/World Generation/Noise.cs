using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    // Generate perlin noise map with octaves
    public static float[] GenerateNoiseMap(int mapSize, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        var noiseMap = new float[mapSize * mapSize];

        var prng = new System.Random(seed);
        var octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float value = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - mapSize / 2f) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - mapSize / 2f) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    value += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (value > maxNoiseHeight)
                    maxNoiseHeight = value;
                else if (value < minNoiseHeight)
                    minNoiseHeight = value;

                //noiseMap[x, y] = value;
                noiseMap[y * mapSize + x] = value;
            }
        }

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                var idx = y * mapSize + x;
                noiseMap[idx] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[idx]);
            }
        }

        return noiseMap;
    }
}