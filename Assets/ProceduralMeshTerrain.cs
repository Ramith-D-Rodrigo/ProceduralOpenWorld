using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class ProceduralMeshTerrain : MonoBehaviour
{
    public const int mapChunkSize = 241;

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

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    float[,] noiseMap;
    Texture2D noiseMapTexture;

    ConcurrentQueue<MapThreadInfo<float[,]>> mapThreadInfos = new ConcurrentQueue<MapThreadInfo<float[,]>>();
    ConcurrentQueue<MapThreadInfo<MeshData>> meshThreadInfos = new ConcurrentQueue<MapThreadInfo<MeshData>>();

    //public AllRequestParams allParams;

    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        //allParams = CreateAllRequestParams();
        //octaves = OctaveGenerator.GenerateOctaves(octaveCount, gain, startAmplitude, startFrequency, lacunarity);
        //noiseMap = Noise.CreateNoiseMap(mapChunkSize, mapChunkSize, seed, new Vector2(xOffSet, yOffSet), scale, octaves);

        if (mapThreadInfos.Count > 0)
        {
            for (int i = 0; i < mapThreadInfos.Count; i++)
            {
                MapThreadInfo<float[,]> threadInfo;
                bool isDequeued = mapThreadInfos.TryDequeue(out threadInfo);
                if(isDequeued)
                {
                    threadInfo.callback(threadInfo.parameter); //parameter is the noiseMap
                }
            }
        }

        //MeshData meshData = MeshGenerator.GenerateMeshData(noiseMap, levelOfDetail, regions, regionHeightCurve, depth);
        //MeshGenerator.CreateMesh(mesh, meshData);

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

        //CreateNoiseMapTexture();       
        //SetShaderGraphVariables();
    }

    private void OnValidate()
    {
        if (octaveCount < 1)
        {
            octaveCount = 1;
        }
    }

    private void CreateNoiseMapTexture()
    {
        noiseMapTexture = new Texture2D(mapChunkSize, mapChunkSize);
        Color[] colors = TextureGenerator.CreateColorMap(mapChunkSize, mapChunkSize, noiseMap, Color.black, Color.white);
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
        List<Octave> octaves = OctaveGenerator.GenerateOctaves(octaveCount, gain, startAmplitude,startFrequency, lacunarity);
        float[,] noiseMap = Noise.CreateNoiseMap(mapChunkSize, mapChunkSize, seed, center + new Vector2(xOffSet, yOffSet), 
            scale, octaves, Noise.NormalizeMode.Global);

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


 /*   private AllRequestParams CreateAllRequestParams()
    {
        MapRequestParams mapRequestParams = new MapRequestParams
        {
            chunkSize = mapChunkSize,
            seed = seed,
            offset = new Vector2(xOffSet, yOffSet),
            scale = scale,
            octaveCount = octaveCount,
            gain = gain,
            startAmplitude = startAmplitude,
            startFrequency = startFrequency,
            lacunarity = lacunarity
        };

        MeshRequestParams meshRequestParams = new MeshRequestParams
        {
            levelOfDetail = levelOfDetail,
            regions = regions,
            regionHeightCurve = regionHeightCurve,
            depth = depth
        };

        AllRequestParams allRequestParams = new AllRequestParams
        {
            mapRequestParams = mapRequestParams,
            meshRequestParams = meshRequestParams
        };

        return allRequestParams;
    }*/
}

/*public struct AllRequestParams
{
    public MapRequestParams mapRequestParams;
    public MeshRequestParams meshRequestParams;

    public AllRequestParams(MapRequestParams mapRequestParams, MeshRequestParams meshRequestParams)
    {
        this.mapRequestParams = mapRequestParams;
        this.meshRequestParams = meshRequestParams;
    }
}*/
