using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTerrain : MonoBehaviour
{
    public int depth = 20;
    public int width = 256;
    public int height = 256;

    public float scale = 20f;

    public float startFrequency = 0.1f;
    public float startAmplitude = 1.0f;
    public float gain = 0.5f;
    public float lacunarity = 2.0f;
    public int octaveCount = 4;
    public float fudgeFactor = 0.1f;

    public float powerValue = 0.1f;

    public float xOffSet = 0.0f;
    public float yOffSet = 0.0f;

    public int seed = 203;

    public List<Octave> octaves = new List<Octave>();

    public List<TerrainType> regions;

    //what cool math functions to be used?
    public bool useSine = false;
    public bool useCosine = false;
    public bool usePow = false;
    public bool useFibonacci = false;
    public bool useGoldenRatio = false;

    public AnimationCurve regionHeightCurve;

    Terrain terrain;
    TerrainLayer[] terrainLayers;
    float[,] heightMap;

    // Start is called before the first frame update
    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainLayers = GenerateTerrainLayers();
        terrain.terrainData.terrainLayers = terrainLayers;
    }

    // Update is called once per frame
    void Update()
    {
        octaves = OctaveGenerator.GenerateOctaves(octaveCount, gain, startAmplitude, startFrequency, lacunarity);
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainLayer[] GenerateTerrainLayers()
    {
        List<Texture2D> textures = new List<Texture2D>();
        TerrainLayer[] terrainLayers = new TerrainLayer[regions.Count];
        for (int i = 0; i < regions.Count; i++)
        {

            Texture2D texture = TextureGenerator.GenerateTextureByTerrainType(width, height, regions[i], 
                xOffSet, yOffSet, octaves, scale, transform.position);
            terrainLayers[i] = new TerrainLayer();
            terrainLayers[i].diffuseTexture = texture;
            terrainLayers[i].tileSize = new Vector2(1, 1);
        }
        return terrainLayers;
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1; // +1 is required for the heightmap to be the same size as the terrain

        terrainData.size = new Vector3(width, depth, height);

        heightMap = GenerateHeightMap();
        terrainData.SetHeights(0, 0, heightMap);
        //set the color
        terrainData.SetAlphamaps(0, 0, GenerateAlphaMap(terrainData));
 
        return terrainData;
    }

    float[,] GenerateHeightMap()
    {
        float[,] heights = new float[width, height];
        Vector2[] randomOffsets = Noise.GenerateRandomOffsets(seed, octaves, xOffSet, yOffSet);

        float maxElevation = float.MinValue;
        float minElevation = float.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = Noise.CalculateElevation(x, y, randomOffsets, octaves, width, height,
                    scale);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float finalElevation = Mathf.InverseLerp(minElevation, maxElevation, heights[x, y]);
                if (regions != null && regions.Count > 0)
                {
                    finalElevation = regionHeightCurve.Evaluate(finalElevation);
                }
                heights[x, y] = finalElevation;
            }
        }

        return heights;
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
