/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class ThreadingHelper
{
    public static Queue<MapThreadInfo> mapThreadInfoQueue = new Queue<MapThreadInfo>();
    public static Queue<MeshThreadInfo> meshThreadInfoQueue = new Queue<MeshThreadInfo>();
    *//*public static void RequestMapData(Action<float[,], Mesh> callBack, MapRequestParams reqParams, Mesh mesh)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callBack, reqParams, mesh);
        };

        new Thread(threadStart).Start();
    }

    public static void OnMapDataReceived(float[,] noiseMap, Mesh mesh)
    {
        Debug.Log("Map data received");
    }*//*

    public static void RequestMeshData(Action<Vector3[], int[], Vector2[]> callBack, float[,] noiseMap, Mesh mesh,
    MeshRequestParams requestParams)
    {
        ThreadStart threadStart = delegate
        {
            //MeshDataThread(callBack, noiseMap, mesh, requestParams);
        };

        new Thread(threadStart).Start();
    }

    public static void OnMeshDataReceived(MeshData meshData)
    {
        Debug.Log("Mesh data received");
    }

    //method running on different thread
*//*    private static void MapDataThread(Action<float[,], Mesh> callBack, MapRequestParams reqParams, Mesh mesh)
    {
        List<Octave> octaves = OctaveGenerator.GenerateOctaves(reqParams.octaveCount, 
            reqParams.gain, reqParams.startAmplitude, reqParams.startFrequency, reqParams.lacunarity);
        float[,] noiseMap = Noise.CreateNoiseMap(reqParams.chunkSize, reqParams.chunkSize, reqParams.seed, 
            reqParams.offset, reqParams.scale, octaves);

        //lock the threadInfoQueue to prevent multiple threads from accessing it at the same time
        lock (mapThreadInfoQueue)
        {
            mapThreadInfoQueue.Enqueue(new MapThreadInfo(callBack, noiseMap, mesh));
        }
    }*/

/*    private static void MeshDataThread(Action<Vector3[], int[], Vector2[]> callBack, float[,] noiseMap, Mesh mesh,
        MeshRequestParams requestParams)
    {
        MeshData meshData = MeshGenerator.GenerateMeshData(noiseMap, requestParams.levelOfDetail, requestParams.regions,
            requestParams.regionHeightCurve, requestParams.depth);
        lock (meshThreadInfoQueue)
        {
            meshThreadInfoQueue.Enqueue(new MeshThreadInfo(callBack, mesh, meshData));
        }
    }*//*
}
*/