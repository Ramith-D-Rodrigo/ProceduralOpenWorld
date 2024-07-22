using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public enum NormalizeMode { Local, Global };
    public static float CalculateElevation(int x, int y, Vector2[] offsets, List<Octave> octaves,
            float width, float height, float scale)
    {
        float elevation = 0.0f;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int i = 0; i < octaves.Count; i++)
        {
            float xCoord = (x - halfWidth + offsets[i].x) / scale * octaves[i].frequency;
            float yCoord = (y - halfHeight + offsets[i].y) / scale * octaves[i].frequency;
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
            float yOff = prnGenerator.Next(-100000, 100000) - yOffSet;
            randomOffsets[i] = new Vector2(xOff, yOff);
        }

        return randomOffsets;
    }

    public static float[,] CreateNoiseMap(int width, int height, int seed, Vector2 offset, float scale,
        List<Octave> octaves, NormalizeMode normalizeMode, float normalizeDividngFactor, float[,] fallOffMap, bool useFalloff)
    {
        float[,] noiseMap = new float[width, height];
        Vector2[] randomOffsets = GenerateRandomOffsets(seed, octaves, offset.x, offset.y);

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxPossibleHeight = 0f;
        float amplitude = octaves[0].amplitude;
        float gain = octaves[1].amplitude / octaves[0].amplitude;

        for (int i = 0; i < octaves.Count; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= gain;
        }


        float maxLocalElevation = float.MinValue;
        float minLocalElevation = float.MaxValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float elevation = CalculateElevation(x, y, randomOffsets, octaves, width, height, scale);
                if (elevation > maxLocalElevation)
                {
                    maxLocalElevation = elevation;
                }
                else if (elevation < minLocalElevation)
                {
                    minLocalElevation = elevation;
                }
                noiseMap[x, y] = elevation;
            }

        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if(normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalElevation, maxLocalElevation, noiseMap[x, y]);
                }
                else
                {
                    if(normalizeDividngFactor == 0.0f)
                    {

                       normalizeDividngFactor = 0.0001f;
                    }
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / normalizeDividngFactor);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }

                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);
                }
            }
        }

         return noiseMap;
    }
}
