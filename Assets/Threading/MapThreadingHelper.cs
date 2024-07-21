/*using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapThreadingHelper : ThreadingHelperInterface<MapThreadInfo, AllRequestParams, MapData>
{
    private ConcurrentQueue<MapThreadInfo> threadInfoQueue = new ConcurrentQueue<MapThreadInfo>();
    public ConcurrentQueue<MapThreadInfo> ThreadInfoQueue
    {
        get
        {
            return threadInfoQueue;
        }
        set
        {
            ThreadInfoQueue = value;
        }
    }

    private MeshThreadingHelper meshThreadingHelper;
    public MeshThreadingHelper MeshThreadingHelper
    {
        get { 
            return meshThreadingHelper;
        }
        set
        {
            meshThreadingHelper = value;
        }
    }

    private AllRequestParams storedParams;

    public void RequestData(Action<MapData> callBack, AllRequestParams reqParams)
    {
        storedParams = reqParams;
        ThreadStart threadStart = delegate
        {
            DataThread(callBack, reqParams);
        };

        new Thread(threadStart).Start();
    }

    public void OnDataReceived(MapData data)
    {
        storedParams.meshRequestParams.mapData = data;
        meshThreadingHelper.RequestData(meshThreadingHelper.OnDataReceived, storedParams.meshRequestParams);
    }

    public void DataThread(Action<MapData> callBack, AllRequestParams reqParams)
    {
        List<Octave> octaves = OctaveGenerator.GenerateOctaves(reqParams.mapRequestParams.octaveCount,
            reqParams.mapRequestParams.gain, reqParams.mapRequestParams.startAmplitude, 
            reqParams.mapRequestParams.startFrequency, reqParams.mapRequestParams.lacunarity);

        float[,] noiseMap = Noise.CreateNoiseMap(reqParams.mapRequestParams.chunkSize, reqParams.mapRequestParams.chunkSize,
            reqParams.mapRequestParams.seed, reqParams.mapRequestParams.offset, reqParams.mapRequestParams.scale, octaves);

        MapData mapData = new MapData(noiseMap);

        //lock the threadInfoQueue to prevent multiple threads from accessing it at the same time
        lock (ThreadInfoQueue)
        {
            ThreadInfoQueue.Enqueue(new MapThreadInfo(callBack, mapData));
        }
    }
}

public struct MapData
{
    public readonly float[,] noiseMap;

    public MapData(float[,] noiseMap)
    {
        this.noiseMap = noiseMap;
    }
}

public struct MapThreadInfo
{
    public readonly Action<MapData> callback;
    public readonly MapData mapData;

    public MapThreadInfo(Action<MapData> callback, MapData mapData)
    {
        this.callback = callback;
        this.mapData = mapData;
    }
}
public struct MapRequestParams
{
    public int chunkSize;
    public int seed;
    public Vector2 offset;
    public float scale;
    public int octaveCount;
    public float gain;
    public float startAmplitude;
    public float startFrequency;
    public float lacunarity;

    public MapRequestParams(int chunkSize, int seed, Vector2 offset, float scale, int octaveCount,
        float gain, float startAmplitude, float startFrequency, float lacunarity)
    {
        this.chunkSize = chunkSize;
        this.seed = seed;
        this.offset = offset;
        this.scale = scale;
        this.octaveCount = octaveCount;
        this.gain = gain;
        this.startAmplitude = startAmplitude;
        this.startFrequency = startFrequency;
        this.lacunarity = lacunarity;
    }
}*/