using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InfiniteTerrain;

public class NPCInitiator : MonoBehaviour
{
    public GameObject fishingGuyPrefab;
    public GameObject kidPrefab;
    public GameObject grannyPrefab;

    public List<GameObject> fishingGuyPool; //instantiated fishing guys (9 for 9 chunks)
    int fishingGuyPoolIndex = 0;

    public List<GameObject> swimmingGirlPool;
    int swimmingGirlPoolIndex = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject SpawnNPC(NPCType npcType, int mapChunkSize, InfiniteTerrain.TerrainChunk terrainChunk, 
        float lowBoundary, float highBoundary)
    {
        GameObject spawnedNPC = null;
        switch (npcType)
        {
            case NPCType.FishingGuy:
                if (fishingGuyPoolIndex >= fishingGuyPool.Count)
                {
                    return null;
                }
                spawnedNPC = AddNPC(mapChunkSize, terrainChunk, lowBoundary, highBoundary, fishingGuyPool, ref fishingGuyPoolIndex, npcType);
                break;
            case NPCType.Kid:
                break;
            case NPCType.Granny:
                break;
            case NPCType.SwimmingGirl:
                if (swimmingGirlPoolIndex >= swimmingGirlPool.Count)
                {
                    return null;
                }
                spawnedNPC = AddNPC(mapChunkSize, terrainChunk, lowBoundary, highBoundary, swimmingGirlPool, ref swimmingGirlPoolIndex, npcType);
                
                //float on water
                Vector3 worldPos = spawnedNPC.transform.position;
                worldPos.y = terrainChunk.Water.transform.position.y - 1.4f;
                spawnedNPC.transform.position = worldPos;
                break;
        }

        return spawnedNPC;
    }

    private GameObject AddNPC(int mapChunkSize, InfiniteTerrain.TerrainChunk terrainChunk, float lowBoundary, float highBoundary,
        List<GameObject> pool, ref int poolIndex, NPCType npcType)
    {
        for (int z = 0; z < mapChunkSize; z++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                Vector3 placementVertex = terrainChunk.MeshFilter.mesh.vertices[z * mapChunkSize + x];
                float comparingHeight = terrainChunk.NoiseMap[x, z];

                if (comparingHeight > lowBoundary && comparingHeight < highBoundary) //near the water
                {
                    GameObject npc = pool[poolIndex];
                    npc.transform.parent = terrainChunk.Parent.transform;
                    npc.transform.localPosition = placementVertex;

                    switch (npcType)
                    {
                        case NPCType.FishingGuy:
                            RotateFishingGuy(npc, mapChunkSize, x, z, terrainChunk.MeshFilter.mesh.vertices, placementVertex);
                            break;

                        case NPCType.SwimmingGirl:
                            RotateRandomly(npc);
                            break;
                    }
                    poolIndex++;
                    return npc;
                }
            }
        }

        return null;
    }

    private void RotateFishingGuy(GameObject NPC, int mapChunkSize, int x, int z, Vector3[] vertices, Vector3 position)
    {
        //find a way to rotate the NPC to face the water
        //use a closer point to comparing height in the noise map
        if (x + 1 < mapChunkSize && z + 1 < mapChunkSize)
        {
            Vector3 nextVertex = vertices[(z + 1) * mapChunkSize + x + 1];
            Vector3 direction = nextVertex - position;
            NPC.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void RotateRandomly(GameObject NPC)
    {
        NPC.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
    }

    public void RemoveNPC(NPCType nPCType)
    {
        switch(nPCType)
        {
            case NPCType.FishingGuy:
                fishingGuyPoolIndex--;
                break;
            case NPCType.Kid:
                break;
            case NPCType.Granny:
                break;
            case NPCType.SwimmingGirl:
                swimmingGirlPoolIndex--;
                break;
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