using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

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
    private Dictionary<string, int> regionNameDictionary = new Dictionary<string, int>();
    public Dictionary<string,int> RegionNameDictionary { get { return regionNameDictionary; } }

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

    //tree prefab
    public GameObject lowPolyTreePrefab;
    public float treeDensity = 0.5f;
    public float treeScale = 1.0f;
    float lowTreeHeightBoundary;
    float highTreeHeightBoundary;

    //cloud prefab
    public GameObject cloudPrefab;
    private GameObject cloud;
    float cloudHeight = 0.8f;


    Dictionary<Vector2, GameObject> instantiatedTrees = new();

    float[,] noiseMap;
    float[,] falloffMap;
    MeshData meshData;
    Texture2D noiseMapTexture;

    ConcurrentQueue<MapThreadInfo<float[,]>> mapThreadInfos = new ConcurrentQueue<MapThreadInfo<float[,]>>();
    ConcurrentQueue<MapThreadInfo<MeshData>> meshThreadInfos = new ConcurrentQueue<MapThreadInfo<MeshData>>();
    ConcurrentQueue<TreeThreadInfo> treeThreadInfos = new ConcurrentQueue<TreeThreadInfo>();

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
        isInGodMode = !useThreading;
    }

    void Start()
    {
        //populate the dictionary
        for(int i = 0; i < regions.Count; i++)
        {
            regionNameDictionary.Add(regions[i].regionName, i);
        }

        mesh = new Mesh();
        if(!useThreading)
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter.mesh = mesh;
        }
        water = null;

        previousValues = new MapGeneratingValues(depth, scale, startFrequency, startAmplitude, gain, 
           lacunarity, octaveCount, xOffSet, yOffSet, seed, treeDensity, treeScale);

        infiniteTerrain = GetComponent<InfiniteTerrain>();

        SetTreeHeighBoundaries();

        if(!useThreading)
        {
            GenerateCompleteMap();
        }
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

            if(treeThreadInfos.Count > 0)
            {
                for (int i = 0; i < treeThreadInfos.Count; i++)
                {
                    TreeThreadInfo threadInfo;
                    bool isDequeued = treeThreadInfos.TryDequeue(out threadInfo);
                    if (isDequeued)
                    {
                        threadInfo.callback(threadInfo.treePositions, threadInfo.treePrefab, threadInfo.noiseMap);
                    }
                }
            }
        }
        else
        {
            ProcessValueChange();
        }

    }

    private void GenerateCompleteMap()
    {
        noiseMap = GenerateMapData(Vector2.zero, normalizeMode);
        meshData = MeshGenerator.GenerateMeshData(noiseMap, previewLOD, regions, regionHeightCurve, depth);
        MeshGenerator.CreateMesh(mesh, meshData);
        CreateNoiseMapTexture();
        AddClouds();
        SetShaderGraphVariables();
        CreateOrEditWater();
        ClearTrees(instantiatedTrees, noiseMap);
        AddTrees();
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
        water.transform.localScale = CalculateLocalScale(boundSizes, water);
        return water;
    }

    private static Vector3 CalculateLocalScale(Vector2 boundSizes, GameObject water)
    {
        //scale it to the size of the map and divide by 10 to make it smaller
        return new Vector3((boundSizes.x + 1) / 10.0f, 1,
            (boundSizes.y + 1) / 10.0f);
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
        if(regions == null || regions.Count == 0)
        {
            return;
        }

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

    void AddClouds()
    {
        if (regions == null || regions.Count == 0)
        {
            return;
        }

        if (cloud == null)
        {
            cloud = Instantiate(cloudPrefab, transform);
            cloud.transform.localScale = new Vector3(23.9f, 1, 23.9f);
        }
        Vector3 cloudPosition = GetCloudPosition(noiseMap, regionHeightCurve, regions, depth);
        cloud.transform.position = cloudPosition;

    }

    private static Vector3 GetCloudPosition(float[,] noiseMap, AnimationCurve regionHeightCurve, List<TerrainType> regions, float depth)
    {
        float evaluatingHeight = regions[regions.Count - 1].height; //Noise.FindMaxValueBetweenRange(noiseMap, regions[regions.Count - 2].height, regions[regions.Count - 1].height);
        float evalutedHeight = regionHeightCurve.Evaluate(evaluatingHeight);
        Vector3 cloudPosition = new Vector3(0, evalutedHeight * depth, 0);
        return cloudPosition;
    }

    public GameObject CreateCloud(Vector2 boundSizes, Transform parent, float[,] terrainMap)
    {
        GameObject cloud = Instantiate(cloudPrefab, parent);
        cloud.transform.localScale = CalculateLocalScale(boundSizes, cloud);
        Vector3 cloudPosition = GetCloudPosition(terrainMap, regionHeightCurve, regions, depth);
        cloud.transform.localPosition = cloudPosition;

        MeshRenderer cloudMeshRenderer = cloud.GetComponent<MeshRenderer>();
        Vector3 cloudWorldPosition = cloud.transform.position;
        Vector2 cloudOffset =new Vector2(
            (cloudWorldPosition.x / (cloud.transform.localScale.x * 10)),
            (cloudWorldPosition.z / (cloud.transform.localScale.z * 10))
            );

        if(cloudOffset.x < 0)
        {
            cloudOffset.x = Mathf.Floor(cloudOffset.x);
        }
        else
        {
            cloudOffset.x = Mathf.Ceil(cloudOffset.x);
        }

        if(cloudOffset.y < 0)
        {
            cloudOffset.y = Mathf.Floor(cloudOffset.y);
        }
        else
        {
            cloudOffset.y = Mathf.Ceil(cloudOffset.y);
        }

        cloudMeshRenderer.material.SetVector("_cloudPosition", new Vector4(-cloudOffset.x, -cloudOffset.y, 0, 0));
        return cloud;
    }

    private void SetTreeHeighBoundaries()
    {
        if(regions == null || regions.Count == 0)
        {
            return;
        }
        lowTreeHeightBoundary = regions[regionNameDictionary["Forest"]].height;
        highTreeHeightBoundary = regions[regionNameDictionary["Mud"]].height;
    }

    public void ClearTrees(Dictionary<Vector2, GameObject> trees, float[,] noiseMap)
    {
        if(regions == null || regions.Count == 0)
        {
            return;
        }


        List<Vector2> removingKeys = new List<Vector2>();

        //go through each tree and check if it's position is still valid on the new map
        foreach (KeyValuePair<Vector2, GameObject> tree in trees)
        {
            Vector2 noiseMapPosition = tree.Key;
            float currentNoise = noiseMap[(int)noiseMapPosition.x, (int)noiseMapPosition.y];
            if (currentNoise < lowTreeHeightBoundary || currentNoise > lowTreeHeightBoundary)
            {
                Destroy(tree.Value);
                removingKeys.Add(noiseMapPosition);
            }
        }

        removingKeys.ForEach(key => trees.Remove(key));
    }

    private void AddTrees()
    {
        if(regions == null || regions.Count == 0)
        {
            return;
        }

        Dictionary<Vector2, Vector3> treePositions =  GenerateTreePositions(treeScale, treeDensity, mapChunkSize, seed, 
            meshData, noiseMap, lowTreeHeightBoundary, highTreeHeightBoundary);
        InstantiateTrees(treePositions, instantiatedTrees, lowPolyTreePrefab, transform);
    }

    public static Dictionary<Vector2, Vector3> GenerateTreePositions(float treeScale, float treeDensity, int mapChunkSize, int seed, 
        MeshData meshData, float[,] noiseMap, float lowTreeHeightBoundary, float highTreeHeightBoundary)
    {
        List<Octave> octaves = OctaveGenerator.GenerateOctaves(3, 0.5f);
        Vector2 offsets = Vector2.zero;
        float[,] treeMap = Noise.CreateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, offsets, treeScale, octaves,
            Noise.NormalizeMode.Local, 1, new float[0,0], false);

        Dictionary<Vector2, Vector3> treePositions = new();

        for (int z = 0; z < mapChunkSize; z++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                //use mesh's vertices to position the tree
                Vector3 treePlacementVertex = meshData.vertices[z * mapChunkSize + x];
                float comparingHeight = noiseMap[x, z];

                if (comparingHeight > lowTreeHeightBoundary && comparingHeight < highTreeHeightBoundary)
                {
                    if (treeMap[x, z] < treeDensity)
                    {
                        treePositions.Add(new Vector2(x, z), treePlacementVertex);
                    }
                }
            }
        }

        return treePositions;
    }

    public static void InstantiateTrees(Dictionary<Vector2, Vector3> treePositions, Dictionary<Vector2, GameObject> instantiatedTrees, 
        GameObject treePrefab, Transform parent)
    {
        foreach (KeyValuePair<Vector2, Vector3> treePos in treePositions)
        {
            GameObject tree = Instantiate(treePrefab, parent);
            tree.transform.localPosition = treePos.Value;
            tree.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            tree.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.2f);
            instantiatedTrees.Add(treePos.Key, tree);
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

    public void RequestTreeData(MeshData meshData, Transform parent, Dictionary<Vector2, GameObject> trees, 
        Action<Dictionary<Vector2, Vector3>, GameObject, float[,]> callBack)

    {
        ThreadStart threadStart = delegate
        {
            TreeDataThread(meshData, parent, trees, callBack);
        };

        new Thread(threadStart).Start();
    }

    public void TreeDataThread(MeshData meshData, Transform parent, Dictionary<Vector2, GameObject> trees,
        Action<Dictionary<Vector2, Vector3>, GameObject, float[,]> callBack)
    {
        Dictionary<Vector2, Vector3> treePositions = GenerateTreePositions(treeScale, treeDensity, mapChunkSize, seed,
                meshData, meshData.originalNoiseMap, lowTreeHeightBoundary, highTreeHeightBoundary);
        lock (treeThreadInfos) {
            treeThreadInfos.Enqueue(new TreeThreadInfo(callBack, treePositions, lowPolyTreePrefab, meshData.originalNoiseMap));
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

    struct TreeThreadInfo
    {
        public readonly Action<Dictionary<Vector2, Vector3>, GameObject, float[,]> callback;
        public readonly Dictionary<Vector2, Vector3> treePositions;
        public readonly GameObject treePrefab;
        public readonly float[,] noiseMap;

        public TreeThreadInfo(Action<Dictionary<Vector2, Vector3>, GameObject, float[,]> callback, 
            Dictionary<Vector2, Vector3> treePositions, GameObject treePrefab, float[,] noiseMap)
        {
            this.callback = callback;
            this.treePositions = treePositions;
            this.treePrefab = treePrefab;
            this.noiseMap = noiseMap;
        }
    }

    public void ProcessValueChange()
    {
        if (previousValues.depth != depth || previousValues.scale != scale || previousValues.startFrequency != startFrequency || 
            previousValues.startAmplitude != startAmplitude || previousValues.gain != gain || 
            previousValues.lacunarity != lacunarity || previousValues.octaveCount != octaveCount || 
            previousValues.xOffSet != xOffSet || previousValues.yOffSet != yOffSet || previousValues.seed != seed)
        {
            previousValues = new MapGeneratingValues(depth, scale, startFrequency, startAmplitude, gain, lacunarity,
                    octaveCount, xOffSet, yOffSet, seed, treeDensity, treeScale);
            if (useThreading)
            {
                infiniteTerrain.OnValuesChanged();
            }
            else
            {
                GenerateCompleteMap();
            }
        }
        else if (previousValues.treeDensity != treeDensity || previousValues.treeScale != treeScale)
        {
            previousValues = new MapGeneratingValues(depth, scale, startFrequency, startAmplitude, gain, lacunarity,
                    octaveCount, xOffSet, yOffSet, seed, treeDensity, treeScale);
            ClearTrees(instantiatedTrees, noiseMap);
            AddTrees();
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
    public float treeDensity;
    public float treeScale;

    public MapGeneratingValues(int depth, float scale, float startFrequency, float startAmplitude, float gain,
        float lacunarity, int octaveCount, float xOffSet, float yOffSet, int seed, float treeDensity, float treeScale)
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
        this.treeDensity = treeDensity;
        this.treeScale = treeScale;
    }
}
