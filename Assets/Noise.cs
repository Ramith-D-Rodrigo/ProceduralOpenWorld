using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static float CalculateHeight(int x, int y, Vector2[] offsets, List<Octave> octaves,
            float width, float height, float scale, float fudgeFactor, float powerValue)
    {
        float finalHeight = 0.0f;

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        float halfWidth = width / 2;
        float halfHeight = height / 2;

        float octaveSum = OctaveGenerator.CalculateAmplitudeSum(octaves);

        for (int i = 0; i < octaves.Count; i++)
        {
            float xCoord = ((float)x - halfWidth) / scale * octaves[i].frequency + offsets[i].x;
            float yCoord = ((float)y - halfHeight) / scale * octaves[i].frequency + offsets[i].y;
            float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
            finalHeight += perlinValue * octaves[i].amplitude;

            if (finalHeight > maxHeight)
            {
                maxHeight = finalHeight;
            }
            else if (finalHeight < minHeight)
            {
                minHeight = finalHeight;
            }
        }

        finalHeight = Mathf.InverseLerp(minHeight, maxHeight, finalHeight);
        //finalHeight = Mathf.Pow(finalHeight * fudgeFactor, powerValue);
        return finalHeight;
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
}
