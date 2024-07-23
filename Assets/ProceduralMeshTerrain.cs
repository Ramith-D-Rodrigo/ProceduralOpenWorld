using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class ProceduralMeshTerrain : MonoBehaviour
{
    public const int mapChunkSize = 239;

    [Range(0,6)]
    public int previewLOD;
    public int depth = 20;

    public float scale = 20f;
    public float startFrequency = 0.1f;
    public float startAmplitude = 1.0f;
    [Range(0,1)]
    public float gain = 0.5f;
    public float lacunarity = 2.0f;
    public int octaveCount = 4;

    public float xOffSet = 0.0f;
    public float yOffSet = 0.0f;
    public int seed = 203;

    public List<Octave> octaves = new List<Octave>();

    public List<TerrainType> regions;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public AnimationCurve regionHeightCurve;

    public bool useFalloff;
    public bool useThreading;
    public Noise.NormalizeMode normalizeMode;
    public float normalizeDividngFactor;

    //water prefab
    public GameObject waterPrefab;
    private GameObject water;

    float[,] noiseMap;
    float[,] falloffMap;
    Texture2D noiseMapTexture;

    ConcurrentQueue<MapThreadInfo<float[,]>> mapThreadInfos = new ConcurrentQueue<MapThreadInfo<float[,]>>();
    ConcurrentQueue<MapThreadInfo<MeshData>> meshThreadInfos = new ConcurrentQueue<MapThreadInfo<MeshData>>();

    InfiniteTerrain infiniteTerrain;

    MapGeneratingValues previousValues;

    bool isInGodMode;
    public bool IsInGodMode
    {
        get
        {
            return isInGodMode;
        }
        set
        {
            isInGodMode = value;
        }
    }

    //public AllRequestParams allParams;

    private void Awake()
    {
        falloffMap = FallOffGenerator.GenerateFalloffMap(mapChunkSize + 2);
        isInGodMode = false;
    }

    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        water = null;

        previousValues = new MapGeneratingValues(depth, scale, startFrequency, startAmplitude, gain, 
           lacunarity, octaveCount, xOffSet, yOffSet, seed);

        infiniteTerrain = GetComponent<InfiniteTerrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if(useThreading)
        {
            if (mapThreadInfos.Count > 0)
            {
                for (int i = 0; i < mapThreadInfos.Count; i++)
                {
                    MapThreadInfo<float[,]> threadInfo;
                    bool isDequeued = mapThreadInfos.TryDequeue(out threadInfo);
                    if (isDequeued)
                    {
                        threadInfo.callback(threadInfo.parameter); //parameter is the noiseMap
                    }
                }
            }

            if (meshThreadInfos.Count > 0)
            {
                for (int i = 0; i < meshThreadInfos.Count; i++)
                {
                    MapThreadInfo<MeshData> threadInfo;
                    bool isDequeued = meshThreadInfos.TryDequeue(out threadInfo);
                    if (isDequeued)
                    {
                        threadInfo.callback(threadInfo.parameter);
                    }
                }
            }
        }
        else
        {
            GenerateCompleteMap();
        }

    }

    private void GenerateCompleteMap()
    {
        noiseMap = GenerateMapData(Vector2.zero, normalizeMode);
        MeshData meshData = MeshGenerator.GenerateMeshData(noiseMap, previewLOD, regions, regionHeightCurve, depth);
        MeshGenerator.CreateMesh(mesh, meshData);
        CreateNoiseMapTexture();
        SetShaderGraphVariables();
        CreateOrEditWater();
    }

    private void OnValidate()
    {
        if (octaveCount < 1)
        {
            octaveCount = 1;
        }
    }

    float[,] GenerateMapData(Vector2 center, Noise.NormalizeMode normalizeMode)
    {
        octaves = OctaveGenerator.GenerateOctaves(octaveCount, gain, startAmplitude, startFrequency, lacunarity);
        //+2 to account for the border vertices
        float[,] noiseMap = Noise.CreateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, center + new Vector2(xOffSet, yOffSet),
            scale, octaves, normalizeMode, normalizeDividngFactor, falloffMap, useFalloff);
        return noiseMap;
    }

    private void CreateNoiseMapTexture()
    {
        noiseMapTexture = new Texture2D(mapChunkSize + 2, mapChunkSize + 2);
        Color[] colors = TextureGenerator.CreateColorMap(mapChunkSize + 2, mapChunkSize + 2, noiseMap, Color.black, Color.white);
        noiseMapTexture.SetPixels(colors);
        noiseMapTexture.Apply();
    }

    private void SetShaderGraphVariables()
    {
        meshRenderer.sharedMaterial.SetTexture("_HeightMap", noiseMapTexture);

        //region heights
        for(int i = 0; i < regions.Count; i++)
        {
            meshRenderer.sharedMaterial.SetFloat("_" + regions[i].regionName + "Height", regions[i].height);
        }
    }

    public static GameObject CreateWater(Vector2 boundSizes, GameObject waterPrefab, AnimationCurve regionHeightCurve, 
        Transform parent)
    {
            Vector3 waterPosition = Vector3.zero;
            waterPosition = GetWaterPosition(waterPosition, regionHeightCurve);

            //create the water
            GameObject water = Instantiate(waterPrefab, parent, false);
            water.transform.localPosition = waterPosition;
            //scale it to the size of the map and divide by 10 to make it smaller
            water.transform.localScale = new Vector3((boundSizes.x + 1) / 10.0f, 1, 
                (boundSizes.y + 1) / 10.0f);
            return water;
    }


    private static Vector3 GetWaterPosition(Vector3 waterPosition, AnimationCurve regionHeightCurve)
    {
        //get the water ending height from the regions
        //in the current implementation, 3rd key is where the water ends
        waterPosition.y = regionHeightCurve.keys[2].time * 10;
        return waterPosition;
    }

    void CreateOrEditWater()
    {
        if(water == null)
        {
            Vector2 boundSizes = new Vector2(mapChunkSize, mapChunkSize);
            water = CreateWater(boundSizes, waterPrefab, regionHeightCurve, this.transform);
        }
        else
        {
            // change the water position to the new map position
            Vector3 waterPosition = this.transform.position;
            waterPosition = GetWaterPosition(waterPosition, regionHeightCurve);
            water.transform.position = waterPosition;
        }
    }


    public void RequestMapData(Vector2 center, Action<float[,]> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callBack);
        };

        new Thread(threadStart).Start();
    }

    //runs on a different thread
    void MapDataThread(Vector2 center, Action<float[,]> callBack)
    {
        float[,] noiseMap = GenerateMapData(center, Noise.NormalizeMode.Global);

        //lock the threadInfoQueue to prevent multiple threads from accessing it at the same time
        lock (mapThreadInfos)
        {
            mapThreadInfos.Enqueue(new MapThreadInfo<float[,]>(callBack, noiseMap));
        }
    }

    public void RequestMeshData(float[,] noiseMap, int levelOfDetail, Action<MeshData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(noiseMap, levelOfDetail, callBack);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(float[,] noiseMap, int levelOfDetail, Action<MeshData> callBack)
    {
        MeshData meshData = MeshGenerator.GenerateMeshData(noiseMap, levelOfDetail, regions, regionHeightCurve, depth);

        lock (meshThreadInfos)
        {
            meshThreadInfos.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
        }
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    public void ProcessValueChange()
    {
        if (previousValues.depth != depth || previousValues.scale != scale || previousValues.startFrequency != startFrequency || 
            previousValues.startAmplitude != startAmplitude || previousValues.gain != gain || 
            previousValues.lacunarity != lacunarity || previousValues.octaveCount != octaveCount || 
            previousValues.xOffSet != xOffSet || previousValues.yOffSet != yOffSet || previousValues.seed != seed)
        {
            if (useThreading)
            {
                previousValues = new MapGeneratingValues(depth, scale, startFrequency, startAmplitude, gain, lacunarity, 
                    octaveCount, xOffSet, yOffSet, seed); 


                infiniteTerrain.OnValuesChanged();
            }
        }
    }
}

public struct MapGeneratingValues
{
    public int depth;
    public float scale;
    public float startFrequency;
    public float startAmplitude;
    public float gain;
    public float lacunarity;
    public int octaveCount;
    public float xOffSet;
    public float yOffSet;
    public int seed;

    public MapGeneratingValues(int depth, float scale, float startFrequency, float startAmplitude, float gain,
        float lacunarity, int octaveCount, float xOffSet, float yOffSet, int seed)
    {
        this.depth = depth;
        this.scale = scale;
        this.startFrequency = startFrequency;
        this.startAmplitude = startAmplitude;
        this.gain = gain;
        this.lacunarity = lacunarity;
        this.octaveCount = octaveCount;
        this.xOffSet = xOffSet;
        this.yOffSet = yOffSet;
        this.seed = seed;
    }
}
