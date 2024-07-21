using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public Texture2D texture;
    public TerrainType type;

    public float scale = 20f;
    public float startFrequency = 0.1f;
    public float startAmplitude = 1.0f;
    public float gain = 0.5f;
    public float lacunarity = 2.0f;
    public int octaveCount = 4;

    public float xOffSet = 0.0f;
    public float yOffSet = 0.0f;

    public int seed = 203;

    private Renderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();

    }

    void Update()
    {
        GenerateTexture();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            string path = Application.dataPath + "/../Assets/Textures/GeneratedTextures/" + type.regionName + ".png";
            SaveTextureToFile(texture, path);
        }
    }


    public void GenerateTexture()
    {
        List<Octave> octaves = OctaveGenerator.GenerateOctaves(octaveCount, gain, startAmplitude, startFrequency, lacunarity);
        texture = GenerateTextureByTerrainType(width, height, type, xOffSet, yOffSet, octaves, scale, Vector2.zero);
        meshRenderer.sharedMaterial.mainTexture = texture;

    }

    public static void SaveTextureToFile(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    // this is a util function to generate a texture based on the terrain type
    public static Texture2D GenerateTextureByTerrainType(int width, int height, TerrainType terrainType, float xOffset, 
        float yOffset, List<Octave> octaves, float scale, Vector2 center)
    {
        Texture2D texture = new(width, height);
        float[,] noiseMap = Noise.CreateNoiseMap(width, height, 0, center + new Vector2(xOffset, yOffset), scale, 
            octaves, Noise.NormalizeMode.Local);
        Color[] colorMap = CreateColorMap(width, height, noiseMap, terrainType.lowColor, terrainType.highColor);

        texture.SetPixels(colorMap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();
        return texture;
    }

    public static Color[] CreateColorMap(int width, int height, float[,] noiseMap, Color lowColor, Color highColor)
    {
        Color[] colors = new Color[width * height];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                colors[z * width + x] = Color.Lerp(lowColor, highColor, noiseMap[x, z]);
            }
        }
        return colors;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string regionName;
    public float height;
    public Color lowColor;
    public Color highColor;
    public Texture2D texture;
}
