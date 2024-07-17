using System.Collections.Generic;
using UnityEngine;

public class OctaveGenerator
{
    public static List<Octave> GenerateOctaves(int count, float gain,
        float startAmplitude = 1f, float startFrequency = 1f, float lacunarity = 2f)
    {
        List<Octave> octaves = new List<Octave>(count);

        //for the first octave
        octaves.Add(new Octave
        {
            amplitude = startAmplitude,
            frequency = startFrequency
        });

        for (int i = 1; i < count; i++)
        {
            octaves.Add(new Octave
            {
                amplitude = octaves[i - 1].amplitude * gain,    //amplitude decreases by gain
                frequency = octaves[i - 1].frequency * lacunarity
            });
        }

        return octaves;
    }

    public static float CalculateAmplitudeSum(List<Octave> octaves)
    {
        float sum = 0;
        foreach (Octave octave in octaves)
        {
            sum += octave.amplitude;
        }
        return sum;
    }
}

[System.Serializable]
public struct Octave
{
    public float amplitude;
    public float frequency;
}
