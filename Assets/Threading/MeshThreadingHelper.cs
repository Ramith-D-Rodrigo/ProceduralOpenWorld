/*using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MeshThreadingHelper : ThreadingHelperInterface<MeshThreadInfo, MeshRequestParams, MeshAndMeshData>
{
    private ConcurrentQueue<MeshThreadInfo> threadInfoQueue = new ConcurrentQueue<MeshThreadInfo>();
    public ConcurrentQueue<MeshThreadInfo> ThreadInfoQueue
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

    private MapThreadingHelper mapThreadingHelper;
    public MapThreadingHelper MapThreadingHelper
    {
        get
        {
            return mapThreadingHelper;
        }
        set
        {
            mapThreadingHelper = value;
        }
    }

    private Mesh producingMesh;
    public Mesh ProducingMesh
    {
        get
        {
            return producingMesh;
        }
        set
        {
            producingMesh = value;
        }
    }

    public void RequestData(Action<MeshAndMeshData> callBack, MeshRequestParams reqParams)
    {
        ThreadStart threadStart = delegate
        {
            DataThread(callBack, reqParams);
        };

        new Thread(threadStart).Start();
    }

    public void OnDataReceived(MeshAndMeshData data)
    {
        MeshGenerator.CreateMesh(data.mesh, data.meshData);
    }

    public void DataThread(Action<MeshAndMeshData> callBack, MeshRequestParams reqParams)
    {
        MeshData meshData = MeshGenerator.GenerateMeshData(reqParams.mapData.noiseMap, reqParams.levelOfDetail,
            reqParams.regions, reqParams.regionHeightCurve, reqParams.depth);
        MeshAndMeshData meshAndMeshData = new MeshAndMeshData(reqParams.producingMesh, meshData);
        lock (threadInfoQueue)
        {
            threadInfoQueue.Enqueue(new MeshThreadInfo(callBack, meshAndMeshData));
        }
    }
}

public struct MeshThreadInfo
{
    public readonly Action<MeshAndMeshData> callback;
    public readonly MeshAndMeshData meshData;

    public MeshThreadInfo(Action<MeshAndMeshData> callback, MeshAndMeshData meshData)
    {
        this.callback = callback;
        this.meshData = meshData;
    }
}

public struct MeshAndMeshData
{
    public Mesh mesh;
    public MeshData meshData;

    public MeshAndMeshData(Mesh mesh, MeshData meshData)
    {
        this.mesh = mesh;
        this.meshData = meshData;
    }
}

public struct MeshRequestParams
{
    public MapData mapData;
    public int levelOfDetail;
    public int depth;
    public AnimationCurve regionHeightCurve;
    public List<TerrainType> regions;
    public Mesh producingMesh;

    public MeshRequestParams(MapData mapData, int levelOfDetail, int depth, AnimationCurve regionHeightCurve,
        List<TerrainType> regions, Mesh producingMesh)
    {
        this.mapData = mapData;
        this.levelOfDetail = levelOfDetail;
        this.depth = depth;
        this.regionHeightCurve = regionHeightCurve;
        this.regions = regions;
        this.producingMesh = producingMesh;
    }

}
*/