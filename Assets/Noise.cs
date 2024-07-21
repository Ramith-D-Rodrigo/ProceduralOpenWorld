using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static float CalculateElevation(int x, int y, Vector2[] offsets, List<Octave> octaves,
            float width, float height, float scale)
    {
        float elevation = 0.0f;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int i = 0; i < octaves.Count; i++)
        {
            float xCoord = (x - halfWidth) / scale * octaves[i].frequency + offsets[i].x;
            float yCoord = (y - halfHeight) / scale * octaves[i].frequency + offsets[i].y;
            float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
            elevation += perlinValue * octaves[i].amplitude;
        }

        return elevation;
    }

    public static Vector2[] GenerateRandomOffsets(int seed, List<Octave> octaves, float xOffSet, float yOffSet)
    {
        System.Random prnGenerator = new System.Random(seed);
        Vector2[] randomOffsets = new Vector2[octaves.Count];
        for (int i = 0; i < octaves.Count; i++)
        {
            float xOff = prnGenerator.Next(-100000, 100000) + xOffSet;
            float yOff = prnGenerator.Next(-100000, 100000) + yOffSet;
            randomOffsets[i] = new Vector2(xOff, yOff);
        }

        return randomOffsets;
    }

    public static float[,] CreateNoiseMap(int width, int height, int seed, Vector2 offset, float scale, 
        List<Octave> octaves, Vector2 center)
    {
        offset = new Vector2(center.x + offset.x, center.y + offset.y);
        float[,] noiseMap = new float[width, height];
        Vector2[] randomOffsets = GenerateRandomOffsets(seed, octaves, offset.x, offset.y);

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxElevation = float.MinValue;
        float minElevation = float.MaxValue;



        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float elevation = CalculateElevation(x, y, randomOffsets, octaves, width, height, scale);
                if (elevation > maxElevation)
                {
                    maxElevation = elevation;
                }
                else if (elevation < minElevation)
                {
                    minElevation = elevation;
                }
                noiseMap[x, y] = elevation;
            }

        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minElevation, maxElevation, noiseMap[x, y]);
            }
        }

         return noiseMap;
    }
}
