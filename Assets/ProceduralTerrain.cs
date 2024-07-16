using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTerrain : MonoBehaviour
{
    public int depth = 20;
    public int width = 256;
    public int height = 256;

    public float scale = 20f;

    public float heightFrequency = 0.1f;
    public float widthFrequencey = 0.1f;

    public float powerValue = 0.1f;

    public float xOffSet = 0.0f;
    public float yOffSet = 0.0f;

    public int seed = 203;

    public List<OctaveGenerator.Octave> octaves = new List<OctaveGenerator.Octave>();

    public List<TerrainType> regions;

    //what cool math functions to be used?
    public bool useSine = false;
    public bool useCosine = false;
    public bool usePow = false;
    public bool useFibonacci = false;
    public bool useGoldenRatio = false;

    Terrain terrain;
    TerrainLayer[] terrainLayers;
    float[,] heightMap;

    // Start is called before the first frame update
    void Start()
    {
        //add atleast one octave
        if (octaves.Count == 0)
        {
            octaves = OctaveGenerator.GenerateOctaves(8, 0.5f, 1f, heightFrequency, widthFrequencey);
        }
        terrain = GetComponent<Terrain>();
        terrainLayers = new TerrainLayer[regions.Count];
        InitializeTerrainLayers();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    private void InitializeTerrainLayers()
    {
        for (int i = 0; i < regions.Count; i++)
        {
            terrainLayers[i] = new TerrainLayer();
            terrainLayers[i].diffuseTexture = new Texture2D(512, 512);
            terrainLayers[i].diffuseTexture.wrapMode = TextureWrapMode.Repeat;
            terrainLayers[i].diffuseTexture.filterMode = FilterMode.Trilinear;
            terrainLayers[i].diffuseTexture.Apply();
            terrainLayers[i].tileSize = new Vector2(15, 15);
            terrainLayers[i].tileOffset = new Vector2(0, 0);
            terrainLayers[i].specular = regions[i].color;
            terrainLayers[i].metallic = 0;
            terrainLayers[i].smoothness = 0.5f;
            terrainLayers[i].normalMapTexture = new Texture2D(512, 512);
            terrainLayers[i].normalMapTexture.wrapMode = TextureWrapMode.Repeat;
            terrainLayers[i].normalMapTexture.filterMode = FilterMode.Trilinear;
            terrainLayers[i].normalMapTexture.Apply();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1; // +1 is required for the heightmap to be the same size as the terrain

        terrainData.size = new Vector3(width, depth, height);

        heightMap = GenerateHeightMap();
        terrainData.SetHeights(0, 0, heightMap);
        terrainData.terrainLayers = terrainLayers;
        //set the color
        terrainData.SetAlphamaps(0, 0, GenerateAlphaMap(terrainData));
 

        return terrainData;
    }

    float[,] GenerateHeightMap()
    {
        float[,] heights = new float[width, height];

        System.Random prnGenerator = new System.Random(seed);
        Vector2[] randomOffsets = new Vector2[octaves.Count];
        for (int i = 0; i < octaves.Count; i++)
        {
            float xOff = prnGenerator.Next(-100000, 100000) + xOffSet;
            float yOff = prnGenerator.Next(-100000, 100000) + yOffSet;
            randomOffsets[i] = new Vector2(xOff, yOff);
        }


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y, randomOffsets);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y, Vector2[] offsets)
    {
        float finalHeight = 0.0f;

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        for (int i = 0; i <  octaves.Count; i++)
        {
            float xCoord = ((float)x - halfWidth) / scale * octaves[i].xFrequency + offsets[i].x;
            float yCoord = ((float)y - halfHeight) / scale * octaves[i].yFrequency + offsets[i].y;
            float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1; // -1 to 1
            finalHeight += perlinValue * octaves[i].amplitude;

            if(finalHeight > maxHeight)
            {
                maxHeight = finalHeight;
            }
            else if(finalHeight < minHeight)
            {
                minHeight = finalHeight;
            }
        }

        finalHeight /= OctaveGenerator.CalculateAmplitudeSum(octaves);
        //finalHeight = Mathf.InverseLerp(minHeight, maxHeight, finalHeight);
        //finalHeight = Mathf.Pow(finalHeight, powerValue);
        return finalHeight;
    }

    float[,,] GenerateAlphaMap(TerrainData terrainData)
    {
        float[,,] alphas = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, regions.Count];
        for (int x = 0; x < terrainData.alphamapWidth; x++)
        {
            for (int y = 0; y < terrainData.alphamapHeight; y++)
            {

                float normalizedX = (float)x / terrainData.alphamapWidth;
                float normalizedY = (float)y / terrainData.alphamapHeight;

                //Debug.Log("normalizedX : " + normalizedX + " normalizedY : " + normalizedY);

                float heightValue = heightMap[Mathf.FloorToInt(normalizedX * width), Mathf.FloorToInt(normalizedY * height)];

                for (int i = 0; i < regions.Count; i++)
                {
                    if (heightValue <= regions[i].height)
                    {
                        alphas[x, y, i] = 1;
                        break;
                    }
                }
            }
        }

        return alphas;
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
