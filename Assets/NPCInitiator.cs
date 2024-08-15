using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static InfiniteTerrain;

public class NPCInitiator : MonoBehaviour
{
    public GameObject fishingGuyPrefab;
    public GameObject kidPrefab;
    public GameObject grannyPrefab;
    public GameObject swimmingGirlPrefab;

    ConcurrentQueue<NPCThreadInfo> npcThreadInfos = new ConcurrentQueue<NPCThreadInfo>();
    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        if (npcThreadInfos.Count > 0)
        {
            for (int i = 0; i < npcThreadInfos.Count; i++)
            {
                NPCThreadInfo nPCThreadInfo;
                bool isDequeued = npcThreadInfos.TryDequeue(out nPCThreadInfo);
                if (isDequeued)
                {
                    nPCThreadInfo.callBack(nPCThreadInfo.positionAndRotation);
                }
            }
        }
        
    }

    public GameObject SpawnNPC(NPCType npcType, Vector3 localPosition, Quaternion rotation, Transform parent)
    {
        GameObject npc = null;
        switch (npcType)
        {
            case NPCType.FishingGuy:
                npc = Instantiate(fishingGuyPrefab, parent);
                npc.transform.localPosition = localPosition;
                npc.transform.rotation = rotation;
                npc.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);   //half the size
                break;

            case NPCType.Kid:
                //npc = Instantiate(kidPrefab, localPosition, rotation, parent);
                break;

            case NPCType.Granny:
                //npc = Instantiate(grannyPrefab, localPosition, rotation, parent);
                break;

            case NPCType.SwimmingGirl:
                npc = Instantiate(swimmingGirlPrefab, parent);
                npc.transform.localPosition = localPosition;
                npc.transform.rotation = rotation;
                break;
        }

        return npc;
    }

    private PositionAndRotation AddNPC(int mapChunkSize, float[,] noiseMap, Vector3[] vertices, Vector3 waterLocalPos, 
        ProceduralMeshTerrain proceduralMeshTerrain, NPCType npcType)
    {
        float lowBoundary = 0.0f;
        float highBoundary = 0.0f;

        switch(npcType)
        {
            case NPCType.FishingGuy:
                TerrainType grass = proceduralMeshTerrain.regions[proceduralMeshTerrain.RegionNameDictionary["Grass"]];
                TerrainType forest = proceduralMeshTerrain.regions[proceduralMeshTerrain.RegionNameDictionary["Forest"]];

                lowBoundary = grass.height;
                highBoundary = forest.height - 0.15f;
            break;

            case NPCType.SwimmingGirl:
                // should be on the water
                //use noise map to determine the position
                TerrainType deepWater = proceduralMeshTerrain.regions[proceduralMeshTerrain.RegionNameDictionary["DeepWater"]];
                TerrainType water = proceduralMeshTerrain.regions[proceduralMeshTerrain.RegionNameDictionary["Water"]];

                lowBoundary = deepWater.height;
                highBoundary = water.height;

            break;
        }

        for (int z = 0; z < mapChunkSize; z++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                Vector3 placementVertex = vertices[z * (mapChunkSize) + x];
                float comparingHeight = noiseMap[x, z];

                if (comparingHeight > lowBoundary && comparingHeight < highBoundary) //near the water
                {
                    if(random.Next(100) % 3 == 0)   //randomize the placement
                    {
                        continue;
                    }
                    Quaternion rotation = Quaternion.identity;
                    switch (npcType)
                    {
                        case NPCType.FishingGuy:
                            rotation = FishingGuyRotation(mapChunkSize, x, z, vertices, placementVertex);
                            break;

                        case NPCType.SwimmingGirl:
                            //float on water
                            placementVertex.y = waterLocalPos.y - 1.4f;
                            rotation = RandomYRotation();
                            break;
                    }

                    return new PositionAndRotation(placementVertex, rotation);
                }
                else
                {
                    //since the region is not near the water, we can skip some vertices
                    x += 5;
                    z += 5;

                    if (x >= mapChunkSize || z >= mapChunkSize)
                    {
                        break;
                    }
                }
            }
        }

        return new PositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private Quaternion FishingGuyRotation(int mapChunkSize, int x, int z, Vector3[] vertices, Vector3 position)
    {
        Quaternion rotation;
        //find a way to rotate the NPC to face the water
        //use a closer point to comparing height in the noise map
        if (x + 1 < mapChunkSize && z + 1 < mapChunkSize)
        {
            Vector3 nextVertex = vertices[(z + 1) * mapChunkSize + x + 1];
            Vector3 direction = nextVertex - position;
            rotation = Quaternion.LookRotation(direction);
            

        }
        else
        {
            Vector3 nextVertex = vertices[(z - 1) * mapChunkSize + x - 1];
            Vector3 direction = nextVertex - position;
            rotation = Quaternion.LookRotation(direction);
        }

        //if this direction is looking upwards, then rotate it
        if (rotation.eulerAngles.x > 0)
        {
            return Quaternion.Euler(0, rotation.eulerAngles.y + 90, 0);
        }
        return rotation;

    }

    private Quaternion RandomYRotation()
    {
        return Quaternion.Euler(0, random.Next(0, 360), 0);
    }

    public void RequestNPCData(int mapChunkSize, float[,] noiseMap, Vector3[] vertices, Vector3 waterLocalPos, ProceduralMeshTerrain proceduralMeshTerrain, Action<Dictionary<NPCType, PositionAndRotation>> callBack)
    {
       ThreadStart threadStart = delegate
       {
            NPCDataThread(mapChunkSize, noiseMap, vertices, waterLocalPos, proceduralMeshTerrain, callBack);
       };

        new Thread(threadStart).Start();
    }

    private void NPCDataThread(int mapChunkSize, float[,] noiseMap, Vector3[] vertices, Vector3 waterLocalPos, ProceduralMeshTerrain proceduralMeshTerrain, Action<Dictionary<NPCType, PositionAndRotation>> callBack)
    {
        PositionAndRotation FishingGuyPosAndRot = AddNPC(mapChunkSize, noiseMap, vertices, waterLocalPos, proceduralMeshTerrain, NPCType.FishingGuy);
        PositionAndRotation swimmingGirlPosAndRot = AddNPC(mapChunkSize, noiseMap, vertices, waterLocalPos, proceduralMeshTerrain, NPCType.SwimmingGirl);

        Dictionary<NPCType, PositionAndRotation> posAndRot = new Dictionary<NPCType, PositionAndRotation>
        {
            { NPCType.FishingGuy, FishingGuyPosAndRot },
            { NPCType.SwimmingGirl, swimmingGirlPosAndRot }
        };

        lock(npcThreadInfos)
        {
            npcThreadInfos.Enqueue(new NPCThreadInfo(posAndRot, callBack));
        }
    }
}

public enum NPCType
{
    FishingGuy,
    Kid,
    Granny,
    SwimmingGirl
}

public struct NPCThreadInfo
{
    public readonly Dictionary<NPCType, PositionAndRotation> positionAndRotation;
    public Action<Dictionary<NPCType, PositionAndRotation>> callBack;

    public NPCThreadInfo(Dictionary<NPCType, PositionAndRotation> positionAndRotation, 
        Action<Dictionary<NPCType, PositionAndRotation>> callBack)
    {
        this.positionAndRotation = positionAndRotation;
        this.callBack = callBack;
    }
}

public struct PositionAndRotation
{
    public readonly Vector3 position;
    public readonly Quaternion rotation;

    public PositionAndRotation(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}