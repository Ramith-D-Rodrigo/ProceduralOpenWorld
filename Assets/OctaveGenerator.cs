using System.Collections.Generic;
using UnityEngine;

public class OctaveGenerator
{
    [System.Serializable]
    public struct Octave
    {
        public float amplitude;
        public float xFrequency;
        public float yFrequency;
    }

    public static List<Octave> GenerateOctaves(int count, float gain,
        float startAmplitude = 1f, float xFrequency = 0.0001f, float yFrequency = 0.0001f)
    {
        List<Octave> octaves = new List<Octave>(count);

        //for the first octave
        octaves.Add(new Octave
        {
            amplitude = startAmplitude,
            xFrequency = xFrequency,
            yFrequency = yFrequency
        });

        for (int i = 1; i < count; i++)
        {
            octaves.Add(new Octave
            {
                amplitude = octaves[i - 1].amplitude * gain,    //amplitude decreases by gain
                xFrequency = xFrequency * Mathf.Pow(2, i),
                yFrequency = yFrequency * Mathf.Pow(2, i)
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
