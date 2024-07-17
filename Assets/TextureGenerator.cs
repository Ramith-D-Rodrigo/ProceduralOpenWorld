using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator
{
    public static Texture2D GenerateTexture(int width, int height, TerrainType terrainType)
    {
        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = terrainType.color;
            }
        }

        texture.SetPixels(colorMap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();
        return texture;
    }

}

[System.Serializable]
public struct TerrainType
{
    public TerrainTypeIndex terrainType;
    public float height;
    public Color color;
}

public enum TerrainTypeIndex
{
    Water,
    Sand,
    Grass,
    Mud,
    Snow
}
